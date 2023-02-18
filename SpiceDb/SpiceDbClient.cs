using Google.Protobuf.Collections;
using SpiceDb.Api;
using SpiceDb.Enum;
using SpiceDb.Models;
using System.Runtime.CompilerServices;

namespace SpiceDb;

public class SpiceDbClient : ISpiceDbClient
{
    // Original code from SpiceDB.Hierarchical

    private readonly Core? _core;

    public SpiceDbClient(string token) : this("https://grpc.authzed.com", token)
    {
    }

    public SpiceDbClient(string serverAddress, string token)
    {
        if (string.IsNullOrEmpty(serverAddress) || string.IsNullOrEmpty(token))
            throw new Exception("Missing server address or token");

        _core = new Core(serverAddress, token);
    }

    /// <summary>
    /// ReadRelationships reads a set of the relationships matching one or more filters.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="subject"></param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<SpiceDb.Models.ReadRelationshipsResponse> ReadRelationshipsAsync(Models.RelationshipFilter resource, Models.RelationshipFilter? subject = null,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        await foreach (var rs in _core!.ReadRelationshipsAsync(resource.Type, resource.OptionalId,
                           resource.OptionalRelation,
                           subject?.Type ?? string.Empty, subject?.OptionalId ?? string.Empty,
                           subject?.OptionalRelation ?? string.Empty, zedToken.ToAuthzedToken(), cacheFreshness))
        {
            yield return rs;
        }
    }

    /// <summary>
    /// WriteRelationships atomically writes and/or deletes a set of specified relationships. An optional set of
    /// preconditions can be provided that must be satisfied for the operation to commit.
    /// </summary>
    /// <param name="relationships"></param>
    /// <param name="optionalPreconditions"></param>
    /// <returns></returns>
    public async Task<ZedToken?> WriteRelationshipsAsync(List<SpiceDb.Models.RelationshipUpdate>? relationships, List<SpiceDb.Models.Precondition>? optionalPreconditions = null)
    {
        if (relationships is null) return null;

        RepeatedField<Authzed.Api.V1.RelationshipUpdate> updateCollection = new();
        RepeatedField<Authzed.Api.V1.Precondition> preconditionCollection = new();

        var updates = relationships?.Select(x => new Authzed.Api.V1.RelationshipUpdate
        {
            Operation = x.Operation switch
            {
                RelationshipUpdateOperation.Delete => Authzed.Api.V1.RelationshipUpdate.Types.Operation.Delete,
                RelationshipUpdateOperation.Upsert => Authzed.Api.V1.RelationshipUpdate.Types.Operation.Touch,
                RelationshipUpdateOperation.Create => Authzed.Api.V1.RelationshipUpdate.Types.Operation.Create,
                _ => Authzed.Api.V1.RelationshipUpdate.Types.Operation.Unspecified
            },
            Relationship = new Authzed.Api.V1.Relationship
            {
                Resource = new Authzed.Api.V1.ObjectReference { ObjectType = x.Relationship.Resource.Type, ObjectId = x.Relationship.Resource.Id },
                Relation = x.Relationship.Relation,
                Subject = new Authzed.Api.V1.SubjectReference
                {
                    Object = new Authzed.Api.V1.ObjectReference { ObjectType = x.Relationship.Subject.Type, ObjectId = x.Relationship.Subject.Id },
                    OptionalRelation = x.Relationship.Subject.Relation
                },
                OptionalCaveat = x.Relationship.OptionalCaveat != null ? new Authzed.Api.V1.ContextualizedCaveat { CaveatName = x.Relationship.OptionalCaveat.Name, Context = x.Relationship.OptionalCaveat.Context.ToStruct() } : null
            }
        }).ToList() ?? new List<Authzed.Api.V1.RelationshipUpdate>();

        updateCollection.AddRange(updates);

        var conditions = optionalPreconditions?.Select(x => new Authzed.Api.V1.Precondition
        {
            Filter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = x.Filter.Type,
                OptionalResourceId = x.Filter.OptionalId,
                OptionalRelation = x.Filter.OptionalRelation,
                OptionalSubjectFilter = x.OptionalSubjectFilter == null ? null : new Authzed.Api.V1.SubjectFilter
                {
                    SubjectType = x.OptionalSubjectFilter.Type,
                    OptionalSubjectId = x.OptionalSubjectFilter.OptionalId,
                    OptionalRelation = new Authzed.Api.V1.SubjectFilter.Types.RelationFilter
                    {
                        Relation = x.OptionalSubjectFilter.OptionalRelation
                    }
                }
            },
            Operation = x.Operation switch
            {
                PreconditionOperation.MustMatch => Authzed.Api.V1.Precondition.Types.Operation.MustMatch,
                PreconditionOperation.MustNotMatch => Authzed.Api.V1.Precondition.Types.Operation.MustNotMatch,
                _ => Authzed.Api.V1.Precondition.Types.Operation.Unspecified
            }
        }).ToList() ?? new();

        preconditionCollection.AddRange(conditions);

        var response = await _core!.WriteRelationshipsAsync(updateCollection, preconditionCollection);

        return response?.WrittenAt.ToSpiceDbToken();
    }

    /// <summary>
    /// DeleteRelationships atomically bulk deletes all relationships matching the provided filter. If no relationships
    /// match, none will be deleted and the operation will succeed. An optional set of preconditions can be provided
    /// that must be satisfied for the operation to commit.
    /// </summary>
    /// <param name="resourceFilter">resourceFilter.Type is required, all other fields are optional</param>
    /// <param name="optionalSubjectFilter">An optional additional subject filter</param>
    /// <param name="optionalPreconditions">An optional set of preconditions can be provided that must be satisfied for the operation to commit.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns></returns>
    public async Task<ZedToken?> DeleteRelationshipsAsync(SpiceDb.Models.RelationshipFilter resourceFilter, Models.RelationshipFilter? optionalSubjectFilter = null, List<SpiceDb.Models.Precondition>? optionalPreconditions = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        RepeatedField<Authzed.Api.V1.Precondition> preconditionCollection = new();

        var conditions = optionalPreconditions?.Select(x => new Authzed.Api.V1.Precondition
        {
            Filter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = x.Filter.Type,
                OptionalResourceId = x.Filter.OptionalId,
                OptionalRelation = x.Filter.OptionalRelation,
                OptionalSubjectFilter = x.OptionalSubjectFilter == null ? null : new Authzed.Api.V1.SubjectFilter
                {
                    SubjectType = x.OptionalSubjectFilter.Type,
                    OptionalSubjectId = x.OptionalSubjectFilter.OptionalId,
                    OptionalRelation = new Authzed.Api.V1.SubjectFilter.Types.RelationFilter
                    {
                        Relation = x.OptionalSubjectFilter.OptionalRelation
                    }
                }
            },
            Operation = x.Operation switch
            {
                PreconditionOperation.MustMatch => Authzed.Api.V1.Precondition.Types.Operation.MustMatch,
                PreconditionOperation.MustNotMatch => Authzed.Api.V1.Precondition.Types.Operation.MustNotMatch,
                _ => Authzed.Api.V1.Precondition.Types.Operation.Unspecified
            }
        }).ToList() ?? new();

        preconditionCollection.AddRange(conditions);

        return (await _core!.DeleteRelationshipsAsync(resourceFilter.Type, resourceFilter.OptionalId, resourceFilter.OptionalRelation,
            optionalSubjectFilter?.Type ?? string.Empty, optionalSubjectFilter?.OptionalId ?? string.Empty,
            optionalSubjectFilter?.OptionalRelation ?? string.Empty, preconditionCollection, deadline, cancellationToken))
	        .ToSpiceDbToken();
    }

    /// <summary>
    /// CheckPermission determines for a given resource whether a subject computes to having a permission or is a direct member of
    /// a particular relation. Contains support for context as well where context objects can be string, bool, double, int, uint, or long.
    /// </summary>
    /// <param name="permission">Permission relationship to evaluate</param>
    /// <param name="context">Additional context information that may be needed for evaluating caveats</param>
    /// <param name="zedToken"></param>
    /// <returns></returns>
    public async Task<PermissionResponse> CheckPermissionAsync(SpiceDb.Models.Permission permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.CheckPermissionAsync(permission.Resource.Type, permission.Resource.Id, permission.Relation, permission.Subject.Type, permission.Subject.Id, context, zedToken.ToAuthzedToken(), cacheFreshness);
    }

    public async Task<PermissionResponse> CheckPermissionAsync(string permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness) => await CheckPermissionAsync(new SpiceDb.Models.Permission(permission), context, zedToken, cacheFreshness);
    public PermissionResponse CheckPermission(SpiceDb.Models.Permission permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness) => CheckPermissionAsync(permission, context, zedToken, cacheFreshness).Result;
    public PermissionResponse CheckPermission(string permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness) => CheckPermissionAsync(new SpiceDb.Models.Permission(permission), context, zedToken, cacheFreshness).Result;

    /// <summary>
    /// ExpandPermissionTree reveals the graph structure for a resource's permission or relation. This RPC does not recurse infinitely
    /// deep and may require multiple calls to fully unnest a deeply nested graph.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="permission"></param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async Task<ExpandPermissionTreeResponse?> ExpandPermissionAsync(ResourceReference resource, string permission, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var response = await _core!.ExpandPermissionAsync(resource.Type, resource.Id, permission, zedToken.ToAuthzedToken(), cacheFreshness);

        if (response == null)
	        return null;

        return new ExpandPermissionTreeResponse
        {
	        ExpandedAt = response.ExpandedAt.ToSpiceDbToken()!,
	        TreeRoot = BuildTree(response.TreeRoot, new PermissionRelationshipTree())
        };
    }

    private PermissionRelationshipTree BuildTree(Authzed.Api.V1.PermissionRelationshipTree original, PermissionRelationshipTree node)
    {
	    node.ExpandedObject = new ResourceReference(original.ExpandedObject.ObjectType, original.ExpandedObject.ObjectId);
	    node.ExpandedRelation = original.ExpandedRelation;
        
        switch (original.TreeTypeCase)
        {
            case Authzed.Api.V1.PermissionRelationshipTree.TreeTypeOneofCase.Intermediate:
	            node.TreeType = TreeType.Intermediate;
	            node.Intermediate = new AlgebraicSubjectSet
	            {
		            Operation = original.Intermediate.Operation switch
		            {
			            Authzed.Api.V1.AlgebraicSubjectSet.Types.Operation.Exclusion => AlgebraicSubjectSetOperation
				            .Exclusion,
			            Authzed.Api.V1.AlgebraicSubjectSet.Types.Operation.Intersection => AlgebraicSubjectSetOperation
				            .Intersection,
			            Authzed.Api.V1.AlgebraicSubjectSet.Types.Operation.Union => AlgebraicSubjectSetOperation.Union,
			            _ => AlgebraicSubjectSetOperation.Unspecified
		            },
		            Children = original.Intermediate.Children
			            .Select(x => BuildTree(x, new PermissionRelationshipTree())).ToList()
	            };
                break;
            case Authzed.Api.V1.PermissionRelationshipTree.TreeTypeOneofCase.Leaf:
	            node.TreeType = TreeType.Leaf;
	            node.Leaf = new DirectSubjectSet();

                foreach (var subject in original.Leaf.Subjects)
                {
	                node.Leaf.Subjects.Add(new ResourceReference(subject.Object.ObjectType, subject.Object.ObjectId, subject.OptionalRelation));
                };

	            break;
        }

        return node;
    }

    /// <summary>
    /// Add or update multiple relationships as a single atomic update
    /// </summary>
    /// <param name="relationships">List of relationships to add</param>
    /// <returns></returns>
    public async Task<ZedToken?> AddRelationshipsAsync(List<SpiceDb.Models.Relationship> relationships)
    {
	    var request = relationships.Select(x => new SpiceDb.Models.RelationshipUpdate
	    {
		    Relationship = x,
		    Operation = RelationshipUpdateOperation.Upsert
	    }).ToList();

	    return await WriteRelationshipsAsync(request);
    }

    /// <summary>
    /// Add or update a relationship
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public async Task<ZedToken> AddRelationshipAsync(SpiceDb.Models.Relationship relation)
    {
        return (await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, relation.Subject.Relation))
	        .ToSpiceDbToken()!;
    }

    public ZedToken AddRelationship(SpiceDb.Models.Relationship relation) => AddRelationshipAsync(relation).Result;
    public async Task<ZedToken> AddRelationshipAsync(string relation) => await AddRelationshipAsync(new SpiceDb.Models.Relationship(relation));
    public ZedToken AddRelationship(string relation) => AddRelationshipAsync(new SpiceDb.Models.Relationship(relation)).Result;

    /// <summary>
    /// Removes an existing relationship (if it exists)
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public async Task<ZedToken> DeleteRelationshipAsync(SpiceDb.Models.Relationship relation)
    {
        return (await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, relation.Subject.Relation, Authzed.Api.V1.RelationshipUpdate.Types.Operation.Delete)).ToSpiceDbToken()!;
    }

    /// <summary>
    /// LookupSubjects returns all the subjects of a given type that have access whether via a computed permission or relation membership.
    /// </summary>
    /// <param name="resource">Resource is the resource for which all matching subjects for the permission or relation will be returned.</param>
    /// <param name="permission">permission is the name of the permission (or relation) for which to find the subjects</param>
    /// <param name="subjectType">subjecttype is the type of subject object for which the IDs will be returned</param>
    /// <param name="optionalSubjectRelation">optionalSubjectRelation is the optional relation for the subject.</param>
    /// <param name="context">context consists of named values that are injected into the caveat evaluation context *</param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<SpiceDb.Models.LookupSubjectsResponse> LookupSubjects(ResourceReference resource,
        string permission,
        string subjectType, string optionalSubjectRelation = "",
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        await foreach (var response in _core!.LookupSubjects(resource.Type, resource.Id, permission, subjectType,
                           optionalSubjectRelation,
                           context, zedToken.ToAuthzedToken(), cacheFreshness))
        {
            yield return response;
        }
    }

    /// <summary>
    /// LookupResources returns all the resources of a given type that a subject can access whether via
    /// a computed permission or relation membership.
    /// </summary>
    /// <param name="resourceType">The type of resource object for which the IDs will be returned.</param>
    /// <param name="permission">The name of the permission or relation for which the subject must check</param>
    /// <param name="subject">The subject with access to the resources</param>
    /// <param name="context">Dictionary of values that are injected into the caveat evaluation context</param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<SpiceDb.Models.LookupResourcesResponse> LookupResources(string resourceType,
        string permission,
        ResourceReference subject,
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        await foreach (var response in _core!.LookupResources(resourceType, permission, 
                           subject.Type, subject.Id, subject.Relation,
                           context, zedToken.ToAuthzedToken(), cacheFreshness))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<SpiceDb.Models.WatchResponse> Watch(List<string>? optionalSubjectTypes = null,
        ZedToken? zedToken = null,
        DateTime? deadline = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in _core!.Watch(optionalSubjectTypes, zedToken.ToAuthzedToken(), deadline, cancellationToken))
        {
            yield return response;
        }
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, ResourceReference subject, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.GetResourcePermissionsAsync(resourceType, permission, subject.Type, subject.Id, zedToken.ToAuthzedToken());
    }

    public string ReadSchema() => _core!.ReadSchemaAsync().Result;

    public async Task ImportSchemaFromFileAsync(string filePath, string prefix = "")
    {
        await ImportSchemaFromStringAsync(await File.ReadAllTextAsync(filePath), prefix);
    }

    /// <summary>
    /// Imports an Authzed Playground compatible schema (not a yaml file, just the commented schema)
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public async Task ImportSchemaFromStringAsync(string schema, string prefix = "")
    {
        if (prefix.Length > 0 && !prefix.EndsWith("/")) prefix += "/";

        var parsedSchema = string.Empty;
        var entities = SchemaParser.Parse(schema).ToList();

        foreach (var entity in entities)
        {
            var def = $"definition {prefix}{entity.ResourceType} " + "{\n";
            Dictionary<string, List<string>> relations = new();

            entity.Relationships.ForEach(relationship =>
            {
                if (!relations.ContainsKey(relationship.Name))
                    relations.Add(relationship.Name, new());

                relations[relationship.Name.Trim()].Add(relationship.SubjectType.Trim());
            });

            foreach (var key in relations.Keys)
            {
                def += $"\trelation {key}: " + String.Join(" | ", relations[key].Select(x => $"{prefix}{x}").ToList()) + "\n";
            }

            foreach (var permission in entity.Permissions)
            {
                def += $"\tpermission {permission.Name} = {permission.Definition}\n";
            }

            def += "}\n\n";

            parsedSchema += def;
        }

        await _core!.WriteSchemaAsync(parsedSchema);
    }

    public async Task<ZedToken?> ImportRelationshipsFromFileAsync(string filePath)
    {
        return await ImportRelationshipsAsync(await File.ReadAllTextAsync(filePath));
    }

    public async Task<ZedToken?> ImportRelationshipsAsync(string content)
    {
        // Read the file as one string.
        Authzed.Api.V1.RelationshipUpdate.Types.Operation operation = Authzed.Api.V1.RelationshipUpdate.Types.Operation.Touch;
        RepeatedField<Authzed.Api.V1.RelationshipUpdate> updateCollection = new RepeatedField<Authzed.Api.V1.RelationshipUpdate>();

        var lines = content.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            string[] cols = System.Text.RegularExpressions.Regex.Split(line.Trim(), ":|@|#");//refer to authzed docs for separator meanings
            if (cols.Length == 5)
            {
                var updateItem = Core.GetRelationshipUpdate(cols[0], cols[1], cols[2], cols[3], cols[4], "", operation);
                Core.UpdateRelationships(ref updateCollection, updateItem);
            }
            else if (cols.Length == 6)//contain an additional column of optional subject relation
            {
                var updateItem = Core.GetRelationshipUpdate(cols[0], cols[1], cols[2], cols[3], cols[4], cols[5], operation);
                Core.UpdateRelationships(ref updateCollection, updateItem);
            }
        }

        return (await _core!.WriteRelationshipsAsync(updateCollection)).WrittenAt.ToSpiceDbToken();
    }
}
