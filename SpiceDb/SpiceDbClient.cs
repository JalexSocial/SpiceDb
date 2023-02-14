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

    private Core? _core;

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

    public async Task<ZedToken> AddRelationAsync(SpiceDb.Models.Relationship relation)
    {
        return await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, relation.Subject.Relation);
    }

    public ZedToken AddRelation(SpiceDb.Models.Relationship relation) => AddRelationAsync(relation).Result;
    public async Task<ZedToken> AddRelationAsync(string relation) => await AddRelationAsync(new SpiceDb.Models.Relationship(relation));
    public ZedToken AddRelation(string relation) => AddRelationAsync(new SpiceDb.Models.Relationship(relation)).Result;

    public async Task<ZedToken> DeleteRelationAsync(SpiceDb.Models.Relationship relation, string optionalSubjectRelation = "")
    {
        return await _core!.UpdateRelationshipAsync(relation.Resource.Type, relation.Resource.Id, relation.Relation, relation.Subject.Type, relation.Subject.Id, optionalSubjectRelation, RelationshipUpdate.Types.Operation.Delete);
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, string subjectType, string subjectId, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.GetResourcePermissionsAsync(resourceType, permission, subjectType, subjectId, zedToken);
    }

    public string ExportSchema()
    {
        return _core!.ReadSchemaAsync().Result;
    }

    public async Task ImportSchemaFromFileAsync(string filePath, string prefix = "")
    {
        await ImportSchemaFromStringAsync(File.ReadAllText(filePath), prefix);
    }

    public async Task ImportSchemaFromStringAsync(string schema, string prefix = "")
    {
        if (prefix.Length > 0 && !prefix.EndsWith("/")) prefix += "/";

        var parsed_schema = string.Empty;
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

            parsed_schema += def;
        }

        await _core!.WriteSchemaAsync(parsed_schema);
    }

    public async Task<WriteRelationshipsResponse> ImportRelationshipsFromFileAsync(string filePath)
    {
        return await ImportRelationshipsAsync(File.ReadAllText(filePath));
    }

    public async Task<WriteRelationshipsResponse> ImportRelationshipsAsync(string content)
    {
        // Read the file as one string.
        RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch;
        RepeatedField<RelationshipUpdate> updateCollection = new RepeatedField<RelationshipUpdate>();

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

        return await _core!.WriteRelationshipsAsync(updateCollection);
    }
}
