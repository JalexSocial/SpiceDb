# SpiceDb

Simple SpiceDb client based on SpiceDB.Hierarchical.UI - Works with AuthZed

https://github.com/MaheshBailwal/SpiceDB.Hierarchical.UI

Available on Nuget at https://www.nuget.org/packages/SpiceDb

## Usage

### Install

Install the package using NuGet  
`Install-Package SpiceDb`


Example Using UserSecrets

```csharp

using Microsoft.Extensions.Configuration;
using SpiceDb.Example;
using SpiceDb.Example.MyObjects;
using SpiceDb.Models;

// This is just to keep the server address and token private
var builder = new ConfigurationBuilder()
	.AddUserSecrets(typeof(Secrets).Assembly)
	.AddEnvironmentVariables();
var configurationRoot = builder.Build();

var secrets = configurationRoot.GetSection("AuthZed").Get<Secrets>();

if (secrets is null)
	throw new ArgumentException("Invalid secrets configuration");

// var serverAddress = "https://grpc.authzed.com";
var client = new SpiceDbClient(secrets.ServerAddress, secrets.Token);

// Add relationship where user:bob is a reader of document:firstdoc
client.AddRelationship("arch/document:firstdoc#reader@arch/user:bob");
client.AddRelationship(new Relationship("arch/document:firstdoc", "reader", "arch/user:jacob"));

// This approach uses a little syntactic sugar to define each of the relations
client.AddRelationship(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

// Check to see if user:bob is in fact now a reader of document:firstdoc
var bobCanRead = client.CheckPermission(new Permission("arch/document:firstdoc#reader@arch/user:bob"));

Console.WriteLine($"Can user bob read document:firstdoc? {bobCanRead.HasPermission}");
// true

// Check to see if user:carmella is in fact now a reader of document:firstdoc
var carmellaCanRead = client.CheckPermission(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

Console.WriteLine($"Can user carmella read document:firstdoc? {carmellaCanRead.HasPermission}");
// true


```


### API Methods

```csharp

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
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns></returns>
    Task<ZedToken?> DeleteRelationshipsAsync(SpiceDb.Models.RelationshipFilter resourceFilter, Models.RelationshipFilter? optionalSubjectFilter = null, List<SpiceDb.Models.Precondition>? optionalPreconditions = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken));

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
    string ReadSchema();
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

```