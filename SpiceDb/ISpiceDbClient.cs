using Authzed.Api.V1;
using SpiceDb.Enum;
using SpiceDb.Models;

namespace SpiceDb;

public interface ISpiceDbClient
{
	/// <summary>
	/// ReadRelationships reads a set of the relationships matching one or more filters.
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="subject"></param>
	/// <param name="zedToken"></param>
	/// <param name="cacheFreshness"></param>
	/// <returns></returns>
	IAsyncEnumerable<SpiceDb.Models.ReadRelationshipsResponse> ReadRelationshipsAsync(Models.RelationshipFilter resource, Models.RelationshipFilter? subject = null,
		ZedToken? zedToken = null,
		CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

	/// <summary>
	/// WriteRelationships atomically writes and/or deletes a set of specified relationships. An optional set of
	/// preconditions can be provided that must be satisfied for the operation to commit.
	/// </summary>
	/// <param name="relationships"></param>
	/// <returns></returns>
	Task<ZedToken?> WriteRelationshipsAsync(List<SpiceDb.Models.RelationshipUpdate>? relationships, List<SpiceDb.Models.Precondition>? preconditions = null);

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

	/// <summary>
	/// ExpandPermissionTree reveals the graph structure for a resource's permission or relation. This RPC does not recurse infinitely
	/// deep and may require multiple calls to fully unnest a deeply nested graph.
	/// </summary>
	/// <param name="resource"></param>
	/// <param name="permission"></param>
	/// <param name="zedToken"></param>
	/// <param name="cacheFreshness"></param>
	/// <returns></returns>
	Task<ExpandPermissionTreeResponse?> ExpandPermissionAsync(ResourceReference resource, string permission, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

	Task<ZedToken> AddRelationshipAsync(SpiceDb.Models.Relationship relation);
	Task<ZedToken> AddRelationshipAsync(string relation);
	ZedToken AddRelationship(SpiceDb.Models.Relationship relation);
	ZedToken AddRelationship(string relation);
	Task<ZedToken> DeleteRelationshipAsync(SpiceDb.Models.Relationship relation);

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
	IAsyncEnumerable<SpiceDb.Models.LookupSubjectsResponse> LookupSubjects(ResourceReference resource,
		string permission,
		string subjectType, string optionalSubjectRelation = "", 
		Dictionary<string, object>? context = null,
		ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

	Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, ResourceReference subject, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
	string ExportSchema();
	Task ImportSchemaFromFileAsync(string filePath, string prefix = "");

	/// <summary>
	/// Imports an Authzed Playground compatible schema (not a yaml file, just the commented schema)
	/// </summary>
	/// <param name="schema"></param>
	/// <param name="prefix"></param>
	/// <returns></returns>
	Task ImportSchemaFromStringAsync(string schema, string prefix = "");

	Task<ZedToken?> ImportRelationshipsFromFileAsync(string filePath);
	Task<ZedToken?> ImportRelationshipsAsync(string content);
}