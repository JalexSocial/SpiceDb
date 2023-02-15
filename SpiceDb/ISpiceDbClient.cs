using Authzed.Api.V1;
using SpiceDb.Enum;
using SpiceDb.Models;

namespace SpiceDb;

public interface ISpiceDbClient
{
	/// <summary>
	/// Checks whether the permission exists or not. Contains support for context as well where context objects
	/// can be string, bool, double, int, uint, or long.
	/// </summary>
	/// <param name="permission">Permission relationship to evaluate</param>
	/// <param name="context">Additional context information that may be needed for evaluating caveats</param>
	/// <param name="zedToken"></param>
	/// <returns></returns>
	Task<PermissionResponse> CheckPermissionAsync(SpiceDb.Models.Permission permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

	Task<PermissionResponse> CheckPermissionAsync(string permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
	PermissionResponse CheckPermission(SpiceDb.Models.Permission permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
	PermissionResponse CheckPermission(string permission, Dictionary<string, object>? context = null, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
    Task<ExpandPermissionTreeResponse?> ExpandPermissionAsync(ResourceReference resource, string permission, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
    Task<ZedToken> AddRelationshipAsync(SpiceDb.Models.Relationship relation);
	Task<ZedToken> AddRelationshipAsync(string relation);
	ZedToken AddRelationship(SpiceDb.Models.Relationship relation);
	ZedToken AddRelationship(string relation);
	Task<ZedToken> DeleteRelationshipAsync(SpiceDb.Models.Relationship relation, string optionalSubjectRelation = "");
	Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, string subjectType, string subjectId, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
	string ExportSchema();
	Task ImportSchemaFromFileAsync(string filePath, string prefix = "");
	Task ImportSchemaFromStringAsync(string schema, string prefix = "");
	Task<WriteRelationshipsResponse> ImportRelationshipsFromFileAsync(string filePath);
	Task<WriteRelationshipsResponse> ImportRelationshipsAsync(string content);
}