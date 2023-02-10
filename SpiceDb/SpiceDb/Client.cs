using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authzed.Api.V1;
using Google.Protobuf.Collections;
using SpiceDb.Api;
using SpiceDb.Enum;
using static System.Formats.Asn1.AsnWriter;

namespace SpiceDb;

// Original code from SpiceDB.Hierarhical
public class Client
{
    private readonly string _serverAddress = string.Empty;
    private readonly string _token = string.Empty;

    private Core? _core;

    public Client(string serverAddress, string token)
    {
        if (string.IsNullOrEmpty(serverAddress) || string.IsNullOrEmpty(token))
            throw new ArgumentNullException("Missing server address or token");

        _serverAddress = serverAddress;
        _token = token;
        _core = new Core(serverAddress, token);
    }

    public string Schema => _core!.ReadSchemaAsync().Result;

    public async Task<bool> CheckPermissionAsync(string resourceType, string resourceId, string permission,
        string subjectType, string subjectId)
    {
        return await _core!.CheckPermissionAsync(resourceType, resourceId, permission, subjectType, subjectId);
    }

    public async Task<ZedToken> AddRelationAsync(string resourceType, string resourceId, string relation,
        string subjectType, string subjectId, string optionalSubjectRelation = "")
    {
        return await _core!.UpdateRelationshipAsync(resourceType, resourceId, relation, subjectType, subjectId, optionalSubjectRelation);
    }

    public async Task<ZedToken> DeleteRelationAsync(string resourceType, string resourceId, string relation,
        string subjectType, string subjectId, string optionalSubjectRelation = "")
    {
        return await _core!.UpdateRelationshipAsync(resourceType, resourceId, relation, subjectType, subjectId, optionalSubjectRelation, RelationshipUpdate.Types.Operation.Delete);
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, string subjectType, string subjectId, ZedToken zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.GetResourcePermissionsAsync(resourceType, permission, subjectType, subjectId, zedToken);
    }

    public async Task ImportSchemaFromFileAsync(string filePath)
    {
        await ImportSchemaFromStringAsync(File.ReadAllText(filePath));
    }

    public async Task ImportSchemaFromStringAsync(string schema)
    {
        await _core!.WriteSchemaAsync(schema);
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
