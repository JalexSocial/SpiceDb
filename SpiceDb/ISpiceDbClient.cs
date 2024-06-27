using SpiceDb.Enum;
using SpiceDb.Models;

namespace SpiceDb;

public interface ISpiceDbClient
{
    /// <summary>
    /// Asynchronously reads a set of relationships matching one or more filters.
    /// </summary>
    /// <param name="resource">The filter to apply to the resource part of the relationships.</param>
    /// <param name="subject">An optional filter to apply to the subject part of the relationships.</param>
    /// <param name="excludePrefix">Indicates whether the prefix should be excluded from the response.</param>
    /// <param name="limit">If non-zero, specifies the limit on the number of relationships to return
    /// before the stream is closed on the server side. By default, the stream will continue
    /// resolving relationships until exhausted or the stream is closed due to the client or a
    /// network issue.</param>
    /// <param name="cursor">If provided indicates the cursor after which results should resume being returned.
    /// The cursor can be found on the ReadRelationshipsResponse object.</param>
    /// <param name="zedToken">An optional ZedToken for specifying a version of the data to read.</param>
    /// <param name="cacheFreshness">Specifies the acceptable freshness of the data to be read from the cache.</param>
    /// <returns>An async enumerable of <see cref="ReadRelationshipsResponse"/> objects matching the specified filters.</returns>
    IAsyncEnumerable<ReadRelationshipsResponse> ReadRelationshipsAsync(RelationshipFilter resource,
        SubjectFilter? subject = null,
        bool excludePrefix = false,
        int limit = 0,
        Cursor? cursor = null,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

    /// <summary>
    /// WriteRelationships atomically writes and/or deletes a set of specified relationships. An optional set of
    /// preconditions can be provided that must be satisfied for the operation to commit.
    /// </summary>
    /// <param name="relationships"></param>
    /// <param name="optionalPreconditions"></param>
    /// <returns></returns>
    Task<ZedToken?> WriteRelationshipsAsync(List<SpiceDb.Models.RelationshipUpdate>? relationships, List<SpiceDb.Models.Precondition>? optionalPreconditions = null);

    /// <summary>
    /// DeleteRelationships atomically bulk deletes all relationships matching the provided filter. If no relationships
    /// match, none will be deleted and the operation will succeed. An optional set of preconditions can be provided
    /// that must be satisfied for the operation to commit.
    /// </summary>
    /// <param name="resourceFilter">resourceFilter.Type is required, all other fields are optional</param>
    /// <param name="optionalSubjectFilter">An optional additional subject filter</param>
    /// <param name="optionalPreconditions">An optional set of preconditions can be provided that must be satisfied for the operation to commit.</param>
    /// <param name="allowPartialDeletions">if true and a limit is specified, will delete matching found
    /// relationships up to the count specified in optional_limit, and no more.</param>
    /// <param name="limit">if non-zero, specifies the limit on the number of relationships to be deleted.
    /// If there are more matching relationships found to be deleted than the limit specified here,
    /// the deletion call will fail with an error to prevent partial deletion. If partial deletion
    /// is needed, specify below that partial deletion is allowed. Partial deletions can be used
    /// in a loop to delete large amounts of relationships in a *non-transactional* manner.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns></returns>
    Task<DeleteRelationshipsResponse> DeleteRelationshipsAsync(RelationshipFilter resourceFilter,
        SubjectFilter? optionalSubjectFilter = null, List<Precondition>? optionalPreconditions = null,
        bool allowPartialDeletions = false, int limit = 0,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// CheckPermission determines for a given resource whether a subject computes to having a permission or is a direct member of
    /// a particular relation. Contains support for context as well where context objects can be string, bool, double, int, uint, or long.
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

    /// <summary>
    /// Add or update multiple relationships as a single atomic update
    /// </summary>
    /// <param name="relationships">List of relationships to add</param>
    /// <returns></returns>
    Task<ZedToken?> AddRelationshipsAsync(List<SpiceDb.Models.Relationship> relationships);

    /// <summary>
    /// Add or update a relationship
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    Task<ZedToken> AddRelationshipAsync(SpiceDb.Models.Relationship relation);

    Task<ZedToken> AddRelationshipAsync(string relation);
    ZedToken AddRelationship(SpiceDb.Models.Relationship relation);
    ZedToken AddRelationship(string relation);

    /// <summary>
    /// Removes an existing relationship (if it exists)
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    Task<ZedToken> DeleteRelationshipAsync(SpiceDb.Models.Relationship relation);

    /// <summary>
    /// Removes an existing relationship (if it exists)
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    Task<ZedToken> DeleteRelationshipAsync(string relation);

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
    IAsyncEnumerable<SpiceDb.Models.LookupResourcesResponse> LookupResources(string resourceType,
        string permission,
        ResourceReference subject,
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

    IAsyncEnumerable<SpiceDb.Models.WatchResponse> Watch(List<string>? optionalSubjectTypes = null,
        ZedToken? zedToken = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission, ResourceReference subject, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
    
    string ReadSchema();
    
    Task<string> ReadSchemaAsync();
    
    Task WriteSchemaAsync(string schema);

    Task ImportSchemaFromFileAsync(string filePath);

    /// <summary>
    /// Imports an Authzed Playground compatible schema (not a yaml file, just the commented schema)
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    Task ImportSchemaFromStringAsync(string schema);

    Task<ZedToken?> ImportRelationshipsFromFileAsync(string filePath);
    Task<ZedToken?> ImportRelationshipsAsync(string content);

    Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(IEnumerable<CheckBulkPermissionsRequestItem> items, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

    Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(IEnumerable<string> permissions, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);

    Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(IEnumerable<Models.Permission> permissions, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness);
}
