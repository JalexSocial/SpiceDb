using System.Runtime.CompilerServices;
using Authzed.Api.V1;
using Google.Protobuf.Collections;
using Grpc.Core;
using SpiceDb.Enum;
using SpiceDb.Models;
using Cursor = SpiceDb.Models.Cursor;
using DeleteRelationshipsResponse = SpiceDb.Models.DeleteRelationshipsResponse;
using Precondition = Authzed.Api.V1.Precondition;
using Relationship = Authzed.Api.V1.Relationship;
using RelationshipUpdate = Authzed.Api.V1.RelationshipUpdate;
using ZedToken = Authzed.Api.V1.ZedToken;

namespace SpiceDb.Api;

internal class SpiceDbPermissions
{
    private readonly PermissionsService.PermissionsServiceClient? _acl;

    public SpiceDbPermissions(ChannelBase channel)
    {
        _acl = new PermissionsService.PermissionsServiceClient(channel);
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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            Permission = permission,
            ResourceObjectType = resourceType,
            Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId } }
        };

        //Server streaming call, reads messages streamed from the service
        using var call = _acl!.LookupResources(req);

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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            Permission = permission,
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId }
        };

        return await _acl!.ExpandPermissionTreeAsync(req);
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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            Permission = permission,
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
            Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId } },
            Context = context?.ToStruct()
        };

        var call = await _acl!.CheckPermissionAsync(req);

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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
        };

        req.Items.AddRange(items);

        var call = await _acl!.CheckBulkPermissionsAsync(req);

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
                Permission = new Models.Permission(new ResourceReference(x.Request.Resource.ObjectType, x.Request.Resource.ObjectId),
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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
            Permission = permission,
            SubjectObjectType = subjectType,
            OptionalSubjectRelation = optionalSubjectRelation,
            Context = context?.ToStruct()
        };
        
        using var call = _acl!.LookupSubjects(req);

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
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            ResourceObjectType = resourceType,
            Permission = permission,
            Subject = new SubjectReference
            {
                Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId },
                OptionalRelation = optionalSubjectRelation
            },
            Context = context?.ToStruct()
        };

        using var call = _acl!.LookupResources(req);

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
     string optionalRelation = "", string optionalSubjectType = "", string optionalSubjectId = "", string? optionalSubjectRelation = null,
     int? limit = null,
     Cursor? cursor = null,
     ZedToken? zedToken = null,
     CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        if (string.IsNullOrEmpty(optionalSubjectType) && !string.IsNullOrEmpty(optionalSubjectId))
        {
            throw new ArgumentException("Optional subject Id cannot be set without required optional subject type");
        }

        ReadRelationshipsRequest req = new ReadRelationshipsRequest()
        {
            Consistency = CreateConsistency(zedToken, cacheFreshness),
            RelationshipFilter = CreateRelationshipFilter(resourceType, optionalResourceId, optionalRelation, optionalSubjectType, optionalSubjectId, optionalSubjectRelation),
            OptionalLimit = limit != null ? Math.Clamp((uint)limit, 0, 1000) : 0,
            OptionalCursor = cursor is null ? null : new Authzed.Api.V1.Cursor
            {
                Token = cursor.Token
            }
        };
        
        using var call = _acl!.ReadRelationships(req);

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
                        : null,
                    optionalExpiresAt: resp.Relationship.OptionalExpiresAt
                    ),
                AfterResultCursor = resp.AfterResultCursor != null ? new Cursor { Token = resp.AfterResultCursor.Token } : null
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

    public async Task<Models.DeleteRelationshipsResponse> DeleteRelationshipsAsync(string resourceType, string optionalResourceId = "", string optionalRelation = "", string optionalSubjectType ="", string optionalSubjectId = "", string? optionalSubjectRelation = null, 
        RepeatedField<Precondition>? optionalPreconditions = null,  
        bool allowPartialDeletions = false,
        int limit = 0,
        DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var req = new DeleteRelationshipsRequest
        {
            OptionalPreconditions = { optionalPreconditions },
            RelationshipFilter = CreateRelationshipFilter(resourceType, optionalResourceId, optionalRelation, optionalSubjectType, optionalSubjectId, optionalSubjectRelation)
        };

        if (allowPartialDeletions)
        {
            req.OptionalLimit = (uint)Math.Clamp(limit, 0, 1000);
            req.OptionalAllowPartialDeletions = allowPartialDeletions;
        }

        var response = await _acl!.DeleteRelationshipsAsync(req, deadline: deadline, cancellationToken: cancellationToken);

        return new DeleteRelationshipsResponse
        {
            DeletedAt = response.DeletedAt?.ToSpiceDbToken(),
            DeletionProgress = response.DeletionProgress switch
            {
                Authzed.Api.V1.DeleteRelationshipsResponse.Types.DeletionProgress.Unspecified => DeletionProgress
                    .Unspecified,
                Authzed.Api.V1.DeleteRelationshipsResponse.Types.DeletionProgress.Complete => DeletionProgress.Complete,
                Authzed.Api.V1.DeleteRelationshipsResponse.Types.DeletionProgress.Partial => DeletionProgress.Partial,
                _ => throw new SwitchExpressionException(response.DeletionProgress)
            }
        };
    }

    public RelationshipUpdate GetRelationshipUpdate(string resourceType, string resourceId,
           string relation, string subjectType, string subjectId, string optionalSubjectRelation = "",
           RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch, Caveat? caveat = null)
    {
        return new RelationshipUpdate
        {
            Operation = operation,
            Relationship = new Relationship()
            {
                Resource = new ObjectReference { ObjectType = resourceType, ObjectId = resourceId },
                Relation = relation,
                Subject = new SubjectReference { Object = new ObjectReference { ObjectType = subjectType, ObjectId = subjectId }, OptionalRelation = optionalSubjectRelation },
                OptionalCaveat = GetCaveat(caveat)
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

        return await _acl!.WriteRelationshipsAsync(req);
    }

    public async Task<ZedToken> UpdateRelationshipAsync(string resourceType, string resourceId, string relation,
           string subjectType, string subjectId, string optionalSubjectRelation = "",
          RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch, Caveat? caveat = null)
    {

        return await UpdateRelationshipsAsync(resourceType, resourceId, new[] { relation }, subjectType, subjectId, optionalSubjectRelation, operation, caveat);
    }

    public async Task<ZedToken> UpdateRelationshipsAsync(string resourceType, string resourceId, IEnumerable<string> relations,
            string subjectType, string subjectId, string optionalSubjectRelation = "",
           RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch, Caveat? caveat = null)
    {
        RepeatedField<RelationshipUpdate> updateCollection = new RepeatedField<RelationshipUpdate>();

        foreach (var relation in relations)
        {
            var updateItem = GetRelationshipUpdate(resourceType, resourceId, relation.ToLowerInvariant(), subjectType, subjectId, optionalSubjectRelation, operation, caveat);
            UpdateRelationships(ref updateCollection, updateItem);
        }

        WriteRelationshipsResponse resp = await WriteRelationshipsAsync(updateCollection);
        return resp.WrittenAt;
    }

    protected ContextualizedCaveat? GetCaveat(Caveat? caveat)
    {
        if (caveat is null)
        {
            return null;
        }

        return new ContextualizedCaveat
        {
            CaveatName = caveat.Name,
            Context = caveat.Context?.ToStruct(),
        };
    }

    private static Authzed.Api.V1.RelationshipFilter CreateRelationshipFilter(string resourceType, string optionalResourceId, string optionalRelation, string optionalSubjectType, string optionalSubjectId, string? optionalSubjectRelation)
    {
        var filter = new Authzed.Api.V1.RelationshipFilter
        {
            ResourceType = resourceType,
            OptionalRelation = optionalRelation,
            OptionalResourceId = optionalResourceId
        };
            
        if (!String.IsNullOrEmpty(optionalSubjectType))
        {
            filter.OptionalSubjectFilter = new Authzed.Api.V1.SubjectFilter { SubjectType = optionalSubjectType, OptionalSubjectId = optionalSubjectId };
            if (optionalSubjectRelation is not null)
            {
                filter.OptionalSubjectFilter.OptionalRelation = new Authzed.Api.V1.SubjectFilter.Types.RelationFilter()
                {
                    Relation = optionalSubjectRelation
                };
            }
        }

        return filter;
    }

    private static Consistency CreateConsistency(ZedToken? zedToken, CacheFreshness cacheFreshness) =>
        (cacheFreshness, zedToken) switch
        {
            (CacheFreshness.AnyFreshness, _) => new Consistency { MinimizeLatency = true },
            (CacheFreshness.MustRefresh, _) => new Consistency { FullyConsistent = true},
            (CacheFreshness.AtLeastAsFreshAs, not null) => new Consistency { AtLeastAsFresh = zedToken },
            (CacheFreshness.AtExactSnapshot, not null) => new Consistency { AtExactSnapshot = zedToken },
            (CacheFreshness.AtExactSnapshot or CacheFreshness.AtLeastAsFreshAs, null) => throw new ArgumentException("ZedToken must be provided when using AtExactSnapshot or AtLeastAsFreshAs"),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheFreshness), cacheFreshness, "Invalid cache freshness value")
        };
}
