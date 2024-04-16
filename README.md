<img src="https://jalexsocial.github.io/spicedb.docs/images/spicedb-logo.png">

SpiceDb is an open-source, Zanzibar-inspired authorization system that provides a robust and scalable solution for managing fine-grained permissions across distributed systems. Its implementation closely follows the principles set out in Google’s Zanzibar paper, adapting them into a practical and deployable system.

SpiceDb was created by [AuthZed](https://authzed.com/) and [documentation](https://authzed.com/docs/spicedb/getting-started/discovering-spicedb) specifically for SpiceDb can be found on their site.

## SpiceDb.net Documentation 
SpiceDb.net was created by Michael Tanczos and has contributions from Pavel Akimov, Mahesh Bailwal, Vinícius Gajo, and others.  Documentation for SpiceDb.net is in progress and can be found here:
[https://jalexsocial.github.io/spicedb.docs/](https://jalexsocial.github.io/spicedb.docs/)

## Usage

### Install

Available on Nuget at https://www.nuget.org/packages/SpiceDb

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
// Create a new client with a prefix of "arch" for all defined objects
var client = new SpiceDbClient(secrets.ServerAddress, secrets.Token, "arch");

// Add relationship where user:bob is a reader of document:firstdoc
// Note that because the schema prefix is set in the client it is not necessary to always prefix every resource definition 
client.AddRelationship("arch/document:firstdoc#reader@arch/user:bob");

// This also works
client.AddRelationship("document:firstdoc#reader@user:kevin");

// Second approach to adding relationships
client.AddRelationship(new Relationship("arch/document:firstdoc", "reader", "arch/user:jacob"));

// This approach uses a little syntactic sugar to define each of the relations
client.AddRelationship(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

// Check to see if user:bob is in fact now a reader of document:firstdoc
var bobCanRead = client.CheckPermission(new Permission("arch/document:firstdoc#reader@arch/user:bob"));

Console.WriteLine($"Can user bob read document:firstdoc? {bobCanRead.HasPermission}");
// true

// This is a similar check but without adding prefixes
var kevinCanRead = client.CheckPermission(new Permission("document:firstdoc#reader@user:bob"));

Console.WriteLine($"Can user kevin read document:firstdoc? {kevinCanRead.HasPermission}");
// true


// Check to see if user:carmella is in fact now a reader of document:firstdoc
var carmellaCanRead = client.CheckPermission(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

Console.WriteLine($"Can user carmella read document:firstdoc? {carmellaCanRead.HasPermission}");
// true


```

## API Coverage

| authzed.api.v1 method  | Implemented |
| ------------- | ------------- |
| ReadRelationships | Yes  |
| WriteRelationships | Yes  |
| DeleteRelationships | Yes  |
| CheckPermission | Yes  |
| ExpandPermissionTree | Yes  |
| LookupResources | Yes  |
| LookupSubjects | Yes  |
| ReadSchema | Yes  |
| WriteSchema | Yes, as Import* methods |
| Watch | Yes  |


### API Methods

# SpiceDbClient Class

The `SpiceDbClient` class provides a client for interacting with Authzed's SpiceDb, offering methods to manipulate permission systems efficiently.

## Constructors

### SpiceDbClient(string token, string schemaPrefix)

Initializes a new instance of the `SpiceDbClient` class using the default Authzed server address.

**Parameters**

- `token` - Token with admin privileges for manipulating the desired permission system.
- `schemaPrefix` - Schema prefix used for the permission system.

### SpiceDbClient(string serverAddress, string token, string schemaPrefix)

Initializes a new instance of the `SpiceDbClient` class with the specified server address, token, and schema prefix.

**Parameters**

- `serverAddress` - The server address of the Authzed server.
- `token` - The token with admin privileges for manipulating the desired permission system.
- `schemaPrefix` - The schema prefix used for the permission system.

**Exceptions**

- `Exception` - Thrown when the server address or token is null or empty, or if the schema prefix does not meet the required format.

## Methods

### ReadRelationshipsAsync

Asynchronously reads a set of relationships matching one or more filters.

**Parameters**

- `resource` - The filter to apply to the resource part of the relationships.
- `subject` (optional) - An optional filter to apply to the subject part of the relationships.
- `excludePrefix` (optional) - Indicates whether the prefix should be excluded from the response.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to read.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be read from the cache.

**Returns**

- An async enumerable of `ReadRelationshipsResponse` objects matching the specified filters.

### WriteRelationshipsAsync

Atomically writes and/or deletes a set of specified relationships, with optional preconditions.

**Parameters**

- `relationships` - A list of relationship updates to apply.
- `optionalPreconditions` (optional) - An optional list of preconditions that must be satisfied for the operation to commit.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken?` indicating the version of the data after the write operation.

### DeleteRelationshipsAsync

Atomically bulk deletes all relationships matching the provided filters, with optional preconditions.

**Parameters**

- `resourceFilter` - The filter to apply to the resource part of the relationships. The resourceFilter.Type is required; all other fields are optional.
- `optionalSubjectFilter` (optional) - An optional additional filter for the subject part of the relationships.
- `optionalPreconditions` (optional) - An optional list of preconditions that must be satisfied for the operation to commit.
- `deadline` (optional) - An optional deadline for the call. The operation will be cancelled if the deadline is reached.
- `cancellationToken` (optional) - An optional token for cancelling the call.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken?` indicating the version of the data after the delete operation.

### CheckPermissionAsync

Checks permissions for a given resource and subject, optionally considering additional context.

**Parameters**

- `permission` - The permission relationship to evaluate.
- `context` (optional) - An optional dictionary providing additional context information for evaluating caveats.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to consider.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be considered from the cache.

**Returns**

- A task representing the asynchronous operation, with a `PermissionResponse` indicating the result of the permission check.

### ExpandPermissionAsync

Expands the permission tree for a resource's permission or relation, revealing the graph structure. This method may require multiple calls to fully unnest a deeply nested graph.

**Parameters**

- `resource` - The resource reference for which to expand the permission tree.
- `permission` - The name of the permission or relation to expand.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to consider.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be considered from the cache.

**Returns**

- A task representing the asynchronous operation, with an `ExpandPermissionTreeResponse?` indicating the result of the expansion operation.

### AddRelationshipsAsync

Adds or updates multiple relationships as a single atomic update.

**Parameters**

- `relationships` - List of relationships to add or update.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken?` indicating the version of the data after the operation.

### AddRelationshipAsync

Adds or updates a single relationship.

**Parameters**

- `relation` - The relationship to add or update.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken` indicating the version of the data after the operation.

### DeleteRelationshipAsync

Removes an existing relationship (if it exists).

**Parameters**

- `relation` - The relationship to remove.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken` indicating the version of the data after the relationship is removed.

### LookupSubjects

Returns all the subjects of a given type that have access, whether via a computed permission or relation membership.

**Parameters**

- `resource` - Resource is the resource for which all matching subjects for the permission or relation will be returned.
- `permission` - Permission is the name of the permission (or relation) for which to find the subjects.
- `subjectType` - SubjectType is the type of subject object for which the IDs will be returned.
- `optionalSubjectRelation` (optional) - OptionalSubjectRelation is the optional relation for the subject.
- `context` (optional) - Context consists of named values that are injected into the caveat evaluation context.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to consider.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be considered from the cache.

**Returns**

- An async enumerable of `LookupSubjectsResponse` objects representing the subjects with access to the specified resource.

### LookupResources

Returns all the resources of a given type that a subject can access, whether via a computed permission or relation membership.

**Parameters**

- `resourceType` - The type of resource object for which the IDs will be returned.
- `permission` - The name of the permission or relation for which the subject must check.
- `subject` - The subject with access to the resources.
- `context` (optional) - Dictionary of values that are injected into the caveat evaluation context.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to consider.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be considered from the cache.

**Returns**

- An async enumerable of `LookupResourcesResponse` objects representing the resources accessible to the specified subject.

### Watch

Listens for changes to specified subjects and returns updates as they occur.

**Parameters**

- `optionalSubjectTypes` (optional) - A list of subject types to watch for changes.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to watch.
- `deadline` (optional) - An optional deadline for the call. The operation will be cancelled if the deadline is reached.
- `cancellationToken` (optional) - An optional token for cancelling the call.

**Returns**

- An async enumerable of `WatchResponse` objects representing the updates to the watched subjects.

### GetResourcePermissionsAsync

Retrieves the list of permissions for a specified resource, permission, and subject.

**Parameters**

- `resourceType` - The type of the resource.
- `permission` - The name of the permission.
- `subject` - The subject for which permissions are being checked.
- `zedToken` (optional) - An optional ZedToken for specifying a version of the data to consider.
- `cacheFreshness` (optional) - Specifies the acceptable freshness of the data to be considered from the cache.

**Returns**

- A task representing the asynchronous operation, with a list of string indicating the permissions for the specified resource.

### ReadSchema

Reads the current schema in use by the SpiceDB.

**Returns**

- A `string` representing the current schema as defined in the SpiceDB.

### ImportSchemaFromFileAsync

Imports a schema into SpiceDB from a specified file.

**Parameters**

- `filePath` - The path to the file containing the schema to import.

**Returns**

- A task representing the asynchronous operation of importing the schema.

### ImportSchemaFromStringAsync

Imports a schema into SpiceDB from a provided string.

**Parameters**

- `schema` - The schema to import, provided as a string.

**Returns**

- A task representing the asynchronous operation of importing the schema.

### ImportRelationshipsFromFileAsync

Imports relationships into SpiceDB from a specified file.

**Parameters**

- `filePath` - The path to the file containing the relationships to import.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken?` indicating the version of the data after the import operation.

### ImportRelationshipsAsync

Imports relationships into SpiceDB from a provided string.

**Parameters**

- `content` - The relationships to import, provided as a string.

**Returns**

- A task representing the asynchronous operation, with a `ZedToken?` indicating the version of the data after the import operation.

### CheckBulkPermissionAsync (IEnumerable<string> permissions)

Checks multiple permissions in bulk for a specified list of permission identifiers.

**Parameters**

- `permissions` - An enumerable of permission identifiers to check.

**Returns**

- A task representing the asynchronous operation, with a `CheckBulkPermissionsResponse?` indicating the results of the bulk permission checks.

### CheckBulkPermissionAsync (IEnumerable<Permission> permissions)

Checks multiple permissions in bulk for a specified list of `Permission` objects.

**Parameters**

- `permissions` - An enumerable of `Permission` objects to check.

**Returns**

- A task representing the asynchronous operation, with a `CheckBulkPermissionsResponse?` indicating the results of the bulk permission checks.

### CheckBulkPermissionAsync (IEnumerable<CheckBulkPermissionsRequestItem> items)

Checks multiple permissions in bulk for a specified list of `CheckBulkPermissionsRequestItem` objects.

**Parameters**

- `items` - An enumerable of `CheckBulkPermissionsRequestItem` objects to check.

**Returns**

- A task representing the asynchronous operation, with a `CheckBulkPermissionsResponse?` indicating the results of the bulk permission checks.


