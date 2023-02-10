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

    public string Schema => _core!.ReadSchema();

    public async Task<bool> CheckPermissionAsync(string resourceType, string resourceId, string permission,
        string subjectType, string subjectId)
    {
        return await _core!.CheckPermissionAsync(resourceType, resourceId, permission, subjectType, subjectId);
    }

    public void AddRelation(string resourceType, string resourceId, string relation,
        string subjectType, string subjectId, string optionalSubjectRelation = "")
    {
        _core!.UpdateRelationship(resourceType, resourceId, relation, subjectType, subjectId, optionalSubjectRelation);
    }

    public void DeleteRelation(string resourceType, string resourceId, string relation,
        string subjectType, string subjectId, string optionalSubjectRelation = "")
    {
        _core!.UpdateRelationship(resourceType, resourceId, relation, subjectType, subjectId, optionalSubjectRelation, RelationshipUpdate.Types.Operation.Delete);
    }

    public async Task<List<string>> GetResourcePermissions(string resourceType, string permission, string subjectType, string subjectId, ZedToken zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _core!.GetResourcePermissionsAsync(resourceType, permission, subjectType, subjectId, zedToken);
    }

    public void ImportSchemaFromFile(string filePath)
    {
        ImportSchemaFromString(File.ReadAllText(filePath));
    }

    public void ImportSchemaFromString(string schema)
    {
        _core!.WriteSchema(schema);
    }

    public WriteRelationshipsResponse ImportRelationshipsFromFile(string filePath)
    {
        return ImportRelationships(File.ReadAllText(filePath));
    }
    public WriteRelationshipsResponse ImportRelationships(string content)
    {
        // Read the file as one string.
        RelationshipUpdate.Types.Operation operation = RelationshipUpdate.Types.Operation.Touch;
        RepeatedField<RelationshipUpdate> updateCollection = new RepeatedField<RelationshipUpdate>();
        RelationshipUpdate updateItem;

        var lines = content.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            string[] cols = System.Text.RegularExpressions.Regex.Split(line.Trim(), ":|@|#");//refer to authzed docs for separator meanings
            if (cols.Length == 5)
            {
                updateItem = Core.GetRelationshipUpdate(cols[0], cols[1], cols[2], cols[3], cols[4], "", operation);
                Core.UpdateRelationships(ref updateCollection, updateItem);
            }
            else if (cols.Length == 6)//contain an additional column of optional subject relation
            {
                updateItem = Core.GetRelationshipUpdate(cols[0], cols[1], cols[2], cols[3], cols[4], cols[5], operation);
                Core.UpdateRelationships(ref updateCollection, updateItem);
            }
        }

        return _core!.WriteRelationships(ref updateCollection);
    }
}
