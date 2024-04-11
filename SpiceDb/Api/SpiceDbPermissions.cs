using Authzed.Api.V1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using SpiceDb.Enum;
using SpiceDb.Models;
using Precondition = Authzed.Api.V1.Precondition;
using Relationship = Authzed.Api.V1.Relationship;
using RelationshipUpdate = Authzed.Api.V1.RelationshipUpdate;
using ZedToken = Authzed.Api.V1.ZedToken;

namespace SpiceDb.Api;

internal class SpiceDbPermissions
{
    private readonly PermissionsService.PermissionsServiceClient? _acl;
    private readonly CallOptions _callOptions;
    private readonly Metadata? _headers;

    public SpiceDbPermissions(ChannelBase channel, CallOptions callOptions, Metadata? headers)
    {
        _acl = new PermissionsService.PermissionsServiceClient(channel);
        _callOptions = callOptions;
        _headers = headers;
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType,
        string permission,
        string subjectType,
        string subjectId,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        LookupResourcesRequest req = new LookupResourcesRequest
        {
            Consistency = new Authzed.Api.V1.Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            Permission = permission,
            ResourceObjectType = resourceType,
            Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId } }
        };

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        //Server streaming call, reads messages streamed from the service
        var call = _acl!.LookupResources(req, _callOptions);

        var list = new List<string>();

        //The IAsyncStreamReader<T>.ReadAllAsync() extension method reads all messages from the response stream
        await foreach (var resp in call.ResponseStream.ReadAllAsync())
        {
            list.Add(resp.ResourceObjectId);
        }
        return list;
    }

    public async Task<Authzed.Api.V1.ExpandPermissionTreeResponse?> ExpandPermissionAsync(string resourceType,
        string resourceId,
        string permission,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var req = new ExpandPermissionTreeRequest
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            Permission = permission,
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId }
        };

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        return await _acl!.ExpandPermissionTreeAsync(req, _callOptions);
    }

    public async Task<PermissionResponse> CheckPermissionAsync(string resourceType,
        string resourceId,
        string permission,
        string subjectType,
        string subjectId,
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var req = new CheckPermissionRequest
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            Permission = permission,
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
            Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId } },
            Context = context?.ToStruct()
        };

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        var call = await _acl!.CheckPermissionAsync(req, _callOptions);

        return new PermissionResponse
        {
            Permissionship = call?.Permissionship switch
            {
                CheckPermissionResponse.Types.Permissionship.NoPermission => Permissionship.NoPermission,
                CheckPermissionResponse.Types.Permissionship.HasPermission => Permissionship.HasPermission,
                CheckPermissionResponse.Types.Permissionship.ConditionalPermission => Permissionship.ConditionalPermission,
                _ => Permissionship.Unspecified
            },
            ZedToken = call?.CheckedAt.ToSpiceDbToken()
        };
    }

	public async Task<SpiceDb.Models.CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(IEnumerable<Authzed.Api.V1.CheckBulkPermissionsRequestItem> items, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
	{
		var req = new CheckBulkPermissionsRequest()
		{
			Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
		};

		req.Items.AddRange(items);

		if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
		{
			req.Consistency.AtLeastAsFresh = zedToken;
		}
		else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
		{
			req.Consistency.FullyConsistent = true;
		}

		var call = await _acl!.CheckBulkPermissionsAsync(req, options: _callOptions);

		if (call == null)
			return null;

		SpiceDb.Models.CheckBulkPermissionsResponse response = new SpiceDb.Models.CheckBulkPermissionsResponse
		{
			CheckedAt = call.CheckedAt.ToSpiceDbToken(),
			Pairs = call.Pairs.Select(x => new Models.CheckBulkPermissions
			{
				Error = x.Error is null
					? null
					: new Models.Status
					{
						Code = x.Error.Code,
						Message = x.Error.Message,
						Details = x.Error.Details.Select(any => (object)any).ToList()
					},
				PartialCaveatInfo = x.Item.PartialCaveatInfo is null
					? null
					: new SpiceDb.Models.PartialCaveatInfo
					{ MissingRequiredContext = x.Item.PartialCaveatInfo.MissingRequiredContext.ToList() },
				Permissionship = x.Item.Permissionship switch
				{
					CheckPermissionResponse.Types.Permissionship.NoPermission => Permissionship.NoPermission,
					CheckPermissionResponse.Types.Permissionship.HasPermission => Permissionship.HasPermission,
					CheckPermissionResponse.Types.Permissionship.ConditionalPermission => Permissionship.ConditionalPermission,
					_ => Permissionship.Unspecified
				},
				Permission = new Models.Permission(new ResourceReference (x.Request.Resource.ObjectType, x.Request.Resource.ObjectId),
					x.Request.Permission, new ResourceReference(x.Request.Subject.Object.ObjectType, x.Request.Subject.Object.ObjectId, x.Request.Subject.OptionalRelation ?? string.Empty)),
				Context = x.Request.Context?.FromStruct() ?? new()
			}).ToList()
		};

		return response;
	}

	public async IAsyncEnumerable<Models.LookupSubjectsResponse> LookupSubjects(string resourceType, string resourceId, string permission,
        string subjectType, string optionalSubjectRelation = "", Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        LookupSubjectsRequest req = new LookupSubjectsRequest
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
            Permission = permission,
            SubjectObjectType = subjectType,
            OptionalSubjectRelation = optionalSubjectRelation,
            Context = context?.ToStruct()
        };

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        var call = _acl!.LookupSubjects(req, _callOptions);

        await foreach (var resp in call.ResponseStream.ReadAllAsync())
        {
            if (resp is null) continue;

            Models.LookupSubjectsResponse response = new Models.LookupSubjectsResponse
            {
                LookedUpAt = resp.LookedUpAt.ToSpiceDbToken(),
                Subject = new SpiceDb.Models.ResolvedSubject
                {
                    Id = resp.Subject.SubjectObjectId,
                    Permissionship = resp.Subject.Permissionship switch
                    {
                        Authzed.Api.V1.LookupPermissionship.HasPermission => Permissionship.HasPermission,
                        Authzed.Api.V1.LookupPermissionship.ConditionalPermission => Permissionship.ConditionalPermission,
                        _ => Permissionship.Unspecified
                    },
                    MissingRequiredContext = resp.Subject.PartialCaveatInfo?.MissingRequiredContext?.Where(x => !String.IsNullOrEmpty(x)).Select(x => x).ToList() ?? new()
                },
                ExcludedSubjects = resp.ExcludedSubjects.Select(rs => new SpiceDb.Models.ResolvedSubject
                {
                    Id = rs.SubjectObjectId,
                    Permissionship = rs.Permissionship switch
                    {
                        Authzed.Api.V1.LookupPermissionship.HasPermission => Permissionship.HasPermission,
                        Authzed.Api.V1.LookupPermissionship.ConditionalPermission => Permissionship.ConditionalPermission,
                        _ => Permissionship.Unspecified
                    },
                    MissingRequiredContext = rs.PartialCaveatInfo?.MissingRequiredContext?.Where(x => !string.IsNullOrEmpty(x)).Select(x => x).ToList() ?? new()
                }).ToList()
            };

            yield return response;
        }
    }

    public async IAsyncEnumerable<SpiceDb.Models.LookupResourcesResponse> LookupResources(string resourceType,
        string permission,
        string subjectType, string subjectId, string optionalSubjectRelation = "",
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        LookupResourcesRequest req = new LookupResourcesRequest
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            ResourceObjectType = resourceType,
            Permission = permission,
            Subject = new SubjectReference
            {
                Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId },
                OptionalRelation = optionalSubjectRelation
            },
            Context = context?.ToStruct()
        };

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        var call = _acl!.LookupResources(req, _callOptions);


        await foreach (var resp in call.ResponseStream.ReadAllAsync())
        {
            if (resp is null) continue;

            var response = new SpiceDb.Models.LookupResourcesResponse
            {
                LookedUpAt = resp.LookedUpAt.ToSpiceDbToken(),
                Permissionship = resp.Permissionship switch
                {
                    Authzed.Api.V1.LookupPermissionship.HasPermission => Permissionship.HasPermission,
                    Authzed.Api.V1.LookupPermissionship.ConditionalPermission => Permissionship.ConditionalPermission,
                    _ => Permissionship.Unspecified
                },
                ResourceId = resp.ResourceObjectId,
                MissingRequiredContext = resp.PartialCaveatInfo?.MissingRequiredContext
                    .Where(x => !String.IsNullOrEmpty(x)).Select(x => x).ToList() ?? new()
            };

            yield return response;
        }
    }

    public async IAsyncEnumerable<SpiceDb.Models.ReadRelationshipsResponse> ReadRelationshipsAsync(string resourceType, string optionalResourceId = "",
     string optionalRelation = "", string optionalSubjectType = "", string optionalSubjectId = "", string optionalSubjectRelation = "",
     ZedToken? zedToken = null,
     CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        if (string.IsNullOrEmpty(optionalSubjectType) && !string.IsNullOrEmpty(optionalSubjectId))
        {
            throw new ArgumentException("Optional subject Id cannot be set without required optional subject type");
        }

        ReadRelationshipsRequest req = new ReadRelationshipsRequest()
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
            RelationshipFilter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = resourceType,
                OptionalRelation = optionalRelation,
                OptionalResourceId = optionalResourceId
            }
        };
        if (!String.IsNullOrEmpty(optionalSubjectType))
        {
            req.RelationshipFilter.OptionalSubjectFilter = new SubjectFilter() { SubjectType = optionalSubjectType, OptionalSubjectId = optionalSubjectId };
            req.RelationshipFilter.OptionalSubjectFilter.OptionalRelation = new SubjectFilter.Types.RelationFilter() { Relation = optionalSubjectRelation };
        }
        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }
        var call = _acl!.ReadRelationships(req, _callOptions);

        await foreach (var resp in call.ResponseStream.ReadAllAsync())
        {
            var response = new SpiceDb.Models.ReadRelationshipsResponse
            {
                Token = resp.ReadAt.ToSpiceDbToken(),
                Relationship = new SpiceDb.Models.Relationship(
                    resource: new ResourceReference(resp.Relationship.Resource.ObjectType, resp.Relationship.Resource.ObjectId),
                    relation: resp.Relationship.Relation,
                    subject: new ResourceReference(resp.Relationship.Subject.Object.ObjectType, resp.Relationship.Subject.Object.ObjectId, resp.Relationship.Subject.OptionalRelation),
                    optionalCaveat: resp.Relationship.OptionalCaveat != null
                        ? new Caveat
                        {
                            Name = resp.Relationship.OptionalCaveat.CaveatName,
                            Context = resp.Relationship.OptionalCaveat.Context.FromStruct()
                        }
                        : null
                    )
            };

            yield return response;
        }
    }


    public bool UpdateRelationships(ref RepeatedField<RelationshipUpdate> updateCollection, RelationshipUpdate updateItem, bool addOrDelete = true)
    {
        if (addOrDelete)
        {
            updateCollection.Add(updateItem);
            return true;
        }
        else
        {
            return updateCollection.Remove(updateItem);
        }
    }

    public async Task<ZedToken?> DeleteRelationshipsAsync(string resourceType, string optionalResourceId = "", string optionalRelation = "", string optionalSubjectType = "", string optionalSubjectId = "", string optionalSubjectRelation = "", RepeatedField<Precondition>? optionalPreconditions = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var req = new DeleteRelationshipsRequest
        {
            OptionalPreconditions = { optionalPreconditions },
            RelationshipFilter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = resourceType,
                OptionalRelation = optionalRelation,
                OptionalResourceId = optionalResourceId
            }
        };
        if (!String.IsNullOrEmpty(optionalSubjectType))
        {
            req.RelationshipFilter.OptionalSubjectFilter = new SubjectFilter() { SubjectType = optionalSubjectType, OptionalSubjectId = optionalSubjectId };
            if (!String.IsNullOrEmpty(optionalSubjectRelation))
            {
                req.RelationshipFilter.OptionalSubjectFilter.OptionalRelation = new SubjectFilter.Types.RelationFilter()
                {
                    Relation = optionalSubjectRelation
                };
            }
        }

        var response = await _acl!.DeleteRelationshipsAsync(req, _headers, deadline: deadline, cancellationToken: cancellationToken);

        return response?.DeletedAt;
    }

    public RelationshipUpdate GetRelationshipUpdate(string resourceType, string resourceId,
           string relation, string subjectType, string subjectId, string optionalSubjectRelation = "",
           RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch)
    {
        return new RelationshipUpdate
        {
            Operation = operation,
            Relationship = new Relationship()
            {
                Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
                Relation = relation,
                Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId }, OptionalRelation = optionalSubjectRelation },
            }
        };
    }


    public async Task<WriteRelationshipsResponse> WriteRelationshipsAsync(RepeatedField<RelationshipUpdate> updateCollection, RepeatedField<Authzed.Api.V1.Precondition>? optionalPreconditions = null)
    {
        WriteRelationshipsRequest req = new WriteRelationshipsRequest()
        {
            Updates = { updateCollection },
            OptionalPreconditions = { optionalPreconditions ?? new() }
        };

        return await _acl!.WriteRelationshipsAsync(req, _callOptions);
    }



    public async Task<ZedToken> UpdateRelationshipAsync(string resourceType, string resourceId, string relation,
           string subjectType, string subjectId, string optionalSubjectRelation = "",
          RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch)
    {

        return await UpdateRelationshipsAsync(resourceType, resourceId, new[] { relation }, subjectType, subjectId, optionalSubjectRelation, operation);
    }

    public async Task<ZedToken> UpdateRelationshipsAsync(string resourceType, string resourceId, IEnumerable<string> relations,
            string subjectType, string subjectId, string optionalSubjectRelation = "",
           RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch)
    {
        RepeatedField<RelationshipUpdate> updateCollection = new RepeatedField<RelationshipUpdate>();

        foreach (var relation in relations)
        {
            var updateItem = GetRelationshipUpdate(resourceType, resourceId, relation.ToLowerInvariant(), subjectType, subjectId, optionalSubjectRelation, operation);
            UpdateRelationships(ref updateCollection, updateItem);
        }

        WriteRelationshipsResponse resp = await WriteRelationshipsAsync(updateCollection);
        return resp.WrittenAt;
    }

}