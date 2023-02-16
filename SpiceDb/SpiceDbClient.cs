using System.Diagnostics;
using Authzed.Api.V1;
using Google.Protobuf.Collections;
using SpiceDb.Api;
using SpiceDb.Enum;
using SpiceDb.Models;


namespace SpiceDb;

// Original code from SpiceDB.Hierarhical
public class SpiceDbClient : ISpiceDbClient
{
    private readonly string _serverAddress;
    private readonly string _token;

    private readonly Core? _core;

    public SpiceDbClient(string token) : this("https://grpc.authzed.com", token)
    {
    }

    public SpiceDbClient(string serverAddress, string token)
    {
        if (string.IsNullOrEmpty(serverAddress) || string.IsNullOrEmpty(token))
            throw new ArgumentNullException("Missing server address or token");

        _serverAddress = serverAddress;
        _token = token;
        _core = new Core(serverAddress, token);
    }

    public string Schema => _core!.ReadSchemaAsync().Result;

    /// <summary>
    /// Checks whether the permission exists or not. Contains support for context as well where context objects
    /// can be string, bool, double, int, uint, or long.
    /// </summary>
    /// <param name="permission">Permission relationship to evaluate</param>
    /// <param name="context">Additional context information that may be needed for evaluating caveats</param>
    /// <param name="zedToken"></param>
    /// <returns></returns>
    public async Task<PermissionResponse> CheckPermissionAsync(SpiceDb.Models.Permission permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.CheckPermissionAsync(permission.Resource.Type, permission.Resource.Id, permission.Relation, permission.Subject.Type, permission.Subject.Id, context, zedToken, cacheFreshness);
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
        return await _core!.ExpandPermissionAsync(resource.Type, resource.Id, permission, zedToken, cacheFreshness);
    }

    public async Task<ZedToken> AddRelationshipAsync(SpiceDb.Models.Relationship relation)
    {
        return await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, relation.Subject.Relation);
    }

    public ZedToken AddRelationship(SpiceDb.Models.Relationship relation) => AddRelationshipAsync(relation).Result;
    public async Task<ZedToken> AddRelationshipAsync(string relation) => await AddRelationshipAsync(new SpiceDb.Models.Relationship(relation));
    public ZedToken AddRelationship(string relation) => AddRelationshipAsync(new SpiceDb.Models.Relationship(relation)).Result;

    public async Task<ZedToken> DeleteRelationshipAsync(SpiceDb.Models.Relationship relation)
    {
        return await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, relation.Subject.Relation, Authzed.Api.V1.RelationshipUpdate.Types.Operation.Delete);
    }

    public async Task<ZedToken?> WriteRelationshipsAsync(List<SpiceDb.Models.RelationshipUpdate>? relationships)
    {
        if (relationships is null) return null;

        RepeatedField<Authzed.Api.V1.RelationshipUpdate> updateCollection = new();

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
                Resource = new ObjectReference { ObjectType = x.Relationship.Resource.Type, ObjectId = x.Relationship.Resource.Id },
                Relation = x.Relationship.Relation,
                Subject = new SubjectReference
                {
                    Object = new ObjectReference { ObjectType = x.Relationship.Subject.Type, ObjectId = x.Relationship.Subject.Id },
                    OptionalRelation = x.Relationship.Subject.Relation
                },
                OptionalCaveat = x.Relationship.OptionalCaveat != null ? new ContextualizedCaveat { CaveatName = x.Relationship.OptionalCaveat.Name, Context = x.Relationship.OptionalCaveat.Context.ToStruct() } : null
            }
        }).ToList() ?? new List<Authzed.Api.V1.RelationshipUpdate>();

        updateCollection.AddRange(updates);

        var response = await _core!.WriteRelationshipsAsync(updateCollection);

        return response?.WrittenAt;
    }

    public async Task<List<SpiceDb.Models.Relationship>> ReadRelationshipsAsync(Models.RelationshipFilter resource, Models.RelationshipFilter? subject = null, 
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var response = await _core!.ReadRelationshipsAsync(resource.Type, resource.OptionalId,resource.OptionalRelation,
            subject?.Type ?? string.Empty, subject?.OptionalId ?? string.Empty, subject?.OptionalRelation ?? string.Empty, zedToken, cacheFreshness);

        return response.Select(x => new SpiceDb.Models.Relationship(
                new ResourceReference(x.Resource.ObjectType, x.Resource.ObjectId),
                x.Relation,
                new ResourceReference(x.Subject.Object.ObjectType, x.Subject.Object.ObjectId, x.Subject.OptionalRelation),
                x.OptionalCaveat != null ? new Caveat { Name = x.OptionalCaveat.CaveatName, Context = x.OptionalCaveat.Context.FromStruct() } : null))
            .ToList();
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
		                   context, zedToken, cacheFreshness))
	    {
		    yield return response;
	    }
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, ResourceReference subject, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.GetResourcePermissionsAsync(resourceType, permission, subject.Type, subject.Id, zedToken);
    }

    public string ExportSchema()
    {
        return _core!.ReadSchemaAsync().Result;
    }

    public async Task ImportSchemaFromFileAsync(string filePath, string prefix = "")
    {
        await ImportSchemaFromStringAsync(File.ReadAllText(filePath), prefix);
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
        return await ImportRelationshipsAsync(File.ReadAllText(filePath));
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

        return (await _core!.WriteRelationshipsAsync(updateCollection)).WrittenAt;
    }
}
