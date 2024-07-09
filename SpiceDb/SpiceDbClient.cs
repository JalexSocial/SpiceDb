using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Gateway.ProtocGenOpenapiv2.Options;
using Grpc.Net.Client;
using Microsoft.Extensions.FileSystemGlobbing;
using SpiceDb.Api;
using SpiceDb.Enum;
using SpiceDb.Models;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;

namespace SpiceDb;

public class SpiceDbClient : ISpiceDbClient
{
    private readonly string _prefix;
    private readonly SpiceDbCore _spiceDbCore;

    /// <summary>
    /// Create a new client with the default Authzed server address
    /// </summary>
    /// <param name="token">Token with admin privileges that can manipulate the desired permission system</param>
    /// <param name="schemaPrefix">Schema prefix used for permission system</param>
    public SpiceDbClient(string token, string schemaPrefix) : this("https://grpc.authzed.com", token, schemaPrefix)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpiceDbClient"/> class with the specified server address, token, and schema prefix.
    /// </summary>
    /// <param name="serverAddress">The server address of the Authzed server.</param>
    /// <param name="token">The token with admin privileges for manipulating the desired permission system.</param>
    /// <param name="schemaPrefix">The schema prefix used for the permission system.</param>
    /// <exception cref="Exception">Thrown when the server address or token is null or empty, or if the schema prefix does not meet the required format.</exception>
    public SpiceDbClient(string serverAddress, string token, string schemaPrefix)
    {
        if (string.IsNullOrEmpty(serverAddress) || !serverAddress.StartsWith("http"))
            throw new ArgumentException("Expecting http:// or https:// in the SpiceDb endpoint.");

        if (!Regex.IsMatch(schemaPrefix, @"^[a-zA-Z0-9_]{3,63}[a-zA-Z0-9]$"))
            throw new Exception(
                "Schema prefixes must be alphanumeric, lowercase, between 4-64 characters and not end in an underscore");

        var channel = CreateDefaultAuthenticatedChannel(serverAddress, token);

        _spiceDbCore = new SpiceDbCore(channel);
        _prefix = schemaPrefix;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpiceDbClient"/> class with the specified grpc channel.
    /// </summary>
    /// <param name="channel">The grpc channel used to connect to server</param>
    /// <param name="schemaPrefix">The schema prefix used for the permission system.</param>
    /// <exception cref="Exception">Thrown when the server address or token is null or empty, or if the schema prefix does not meet the required format.</exception>
    public SpiceDbClient(ChannelBase channel, string schemaPrefix)
    {
        if (!Regex.IsMatch(schemaPrefix, @"^[a-zA-Z0-9_]{3,63}[a-zA-Z0-9]$"))
            throw new Exception(
                "Schema prefixes must be alphanumeric, lowercase, between 4-64 characters and not end in an underscore");

        _spiceDbCore = new SpiceDbCore(channel);
        _prefix = schemaPrefix;
    }

    private ChannelBase CreateDefaultAuthenticatedChannel(string address, string? token)
    {
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        //Support proxy by setting webproxy on httpClient
        HttpClient.DefaultProxy = new WebProxy();

        var isSecure = address.StartsWith("https://");

        // SslCredentials is used here because this channel is using TLS.
        // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
        var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(isSecure ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure, credentials),
            UnsafeUseInsecureChannelCallCredentials = !isSecure
        });

        return channel;
    }

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
    public async IAsyncEnumerable<ReadRelationshipsResponse> ReadRelationshipsAsync(RelationshipFilter resource,
        SubjectFilter? subject = null,
        bool excludePrefix = false,
        int limit = 0,
        Cursor? cursor = null,
        ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var token = zedToken.ToAuthzedToken();

        await foreach (var rs in _spiceDbCore.Permissions.ReadRelationshipsAsync(EnsurePrefix(resource.Type)!,
                           resource.OptionalId,
                           resource.OptionalRelation,
                           EnsurePrefix(subject?.Type) ?? string.Empty, subject?.OptionalId ?? string.Empty,
                           subject?.OptionalRelation, limit, cursor, token, cacheFreshness))
        {
            if (excludePrefix)
            {
                rs.Relationship.Resource = rs.Relationship.Resource.ExcludePrefix(_prefix);
                rs.Relationship.Subject = rs.Relationship.Subject.ExcludePrefix(_prefix);
            }

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
    public async Task<ZedToken?> WriteRelationshipsAsync(List<RelationshipUpdate>? relationships,
        List<Precondition>? optionalPreconditions = null)
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
                Resource = new Authzed.Api.V1.ObjectReference
                { ObjectType = EnsurePrefix(x.Relationship.Resource.Type), ObjectId = x.Relationship.Resource.Id },
                Relation = x.Relationship.Relation,
                Subject = new Authzed.Api.V1.SubjectReference
                {
                    Object = new Authzed.Api.V1.ObjectReference
                    {
                        ObjectType = EnsurePrefix(x.Relationship.Subject.Type),
                        ObjectId = x.Relationship.Subject.Id
                    },
                    OptionalRelation = x.Relationship.Subject.Relation
                },
                OptionalCaveat = x.Relationship.OptionalCaveat != null
                    ? new Authzed.Api.V1.ContextualizedCaveat
                    {
                        CaveatName = EnsurePrefix(x.Relationship.OptionalCaveat.Name)!,
                        Context = x.Relationship.OptionalCaveat.Context.ToStruct()
                    }
                    : null
            }
        }).ToList() ?? new List<Authzed.Api.V1.RelationshipUpdate>();

        updateCollection.AddRange(updates);

        var conditions = optionalPreconditions?.Select(x => new Authzed.Api.V1.Precondition
        {
            Filter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = EnsurePrefix(x.Filter.Type),
                OptionalResourceId = x.Filter.OptionalId,
                OptionalRelation = x.Filter.OptionalRelation,
                OptionalSubjectFilter = x.OptionalSubjectFilter == null
                    ? null
                    : new Authzed.Api.V1.SubjectFilter
                    {
                        SubjectType = EnsurePrefix(x.OptionalSubjectFilter.Type),
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
        }).ToList() ?? new List<Authzed.Api.V1.Precondition>();

        preconditionCollection.AddRange(conditions);

        var response = await _spiceDbCore.Permissions.WriteRelationshipsAsync(updateCollection, preconditionCollection);

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
    public async Task<DeleteRelationshipsResponse> DeleteRelationshipsAsync(RelationshipFilter resourceFilter,
        SubjectFilter? optionalSubjectFilter = null, List<Precondition>? optionalPreconditions = null,
        bool allowPartialDeletions = false, int limit = 0,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        RepeatedField<Authzed.Api.V1.Precondition> preconditionCollection = new();

        var conditions = optionalPreconditions?.Select(x => new Authzed.Api.V1.Precondition
        {
            Filter = new Authzed.Api.V1.RelationshipFilter
            {
                ResourceType = EnsurePrefix(x.Filter.Type),
                OptionalResourceId = x.Filter.OptionalId,
                OptionalRelation = x.Filter.OptionalRelation,
                OptionalSubjectFilter = x.OptionalSubjectFilter == null
                    ? null
                    : new Authzed.Api.V1.SubjectFilter
                    {
                        SubjectType = EnsurePrefix(x.OptionalSubjectFilter.Type),
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
        }).ToList() ?? new List<Authzed.Api.V1.Precondition>();

        preconditionCollection.AddRange(conditions);

        return (await _spiceDbCore.Permissions.DeleteRelationshipsAsync(EnsurePrefix(resourceFilter.Type)!,
            resourceFilter.OptionalId, resourceFilter.OptionalRelation,
            EnsurePrefix(optionalSubjectFilter?.Type) ?? string.Empty,
            optionalSubjectFilter?.OptionalId ?? string.Empty,
            optionalSubjectFilter?.OptionalRelation, preconditionCollection,
            allowPartialDeletions, limit,
            deadline,
            cancellationToken));
    }

    /// <summary>
    /// CheckPermission determines for a given resource whether a subject computes to having a permission or is a direct member of
    /// a particular relation. Contains support for context as well where context objects can be string, bool, double, int, uint, or long.
    /// </summary>
    /// <param name="permission">Permission relationship to evaluate</param>
    /// <param name="context">Additional context information that may be needed for evaluating caveats</param>
    /// <param name="zedToken"></param>
    /// <returns></returns>
    public async Task<PermissionResponse> CheckPermissionAsync(Models.Permission permission,
        Dictionary<string, object>? context = null, ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _spiceDbCore.Permissions.CheckPermissionAsync(EnsurePrefix(permission.Resource.Type)!,
            permission.Resource.Id, permission.Relation, EnsurePrefix(permission.Subject.Type)!, permission.Subject.Id,
            context, zedToken.ToAuthzedToken(), cacheFreshness);
    }

    public async Task<PermissionResponse> CheckPermissionAsync(string permission,
        Dictionary<string, object>? context = null, ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await CheckPermissionAsync(new Models.Permission(permission), context, zedToken, cacheFreshness);
    }

    public PermissionResponse CheckPermission(Models.Permission permission, Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return CheckPermissionAsync(permission, context, zedToken, cacheFreshness).Result;
    }

    public PermissionResponse CheckPermission(string permission, Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return CheckPermissionAsync(new Models.Permission(permission), context, zedToken, cacheFreshness).Result;
    }

    /// <summary>
    /// Expands the permission tree for a resource's permission or relation, revealing the graph structure. This method may require multiple calls to fully unnest a deeply nested graph.
    /// </summary>
    /// <param name="resource">The resource reference for which to expand the permission tree.</param>
    /// <param name="permission">The name of the permission or relation to expand.</param>
    /// <param name="zedToken">An optional ZedToken for specifying a version of the data to consider.</param>
    /// <param name="cacheFreshness">Specifies the acceptable freshness of the data to be considered from the cache.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="ExpandPermissionTreeResponse?"/> indicating the result of the expansion operation.</returns>
    public async Task<ExpandPermissionTreeResponse?> ExpandPermissionAsync(ResourceReference resource,
        string permission, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var response = await _spiceDbCore.Permissions.ExpandPermissionAsync(EnsurePrefix(resource.Type)!, resource.Id,
            permission, zedToken.ToAuthzedToken(), cacheFreshness);

        if (response == null)
            return null;

        return new ExpandPermissionTreeResponse
        {
            ExpandedAt = response.ExpandedAt.ToSpiceDbToken()!,
            TreeRoot = BuildTree(response.TreeRoot, new PermissionRelationshipTree())
        };
    }

    private PermissionRelationshipTree BuildTree(Authzed.Api.V1.PermissionRelationshipTree original,
        PermissionRelationshipTree node)
    {
        node.ExpandedObject =
            new ResourceReference(original.ExpandedObject.ObjectType, original.ExpandedObject.ObjectId);
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
                    node.Leaf.Subjects.Add(new ResourceReference(subject.Object.ObjectType, subject.Object.ObjectId,
                        subject.OptionalRelation));
                ;

                break;
        }

        return node;
    }

    /// <summary>
    /// Add or update multiple relationships as a single atomic update
    /// </summary>
    /// <param name="relationships">List of relationships to add</param>
    /// <returns></returns>
    public async Task<ZedToken?> AddRelationshipsAsync(List<Relationship> relationships)
    {
        var request = relationships.Select(x => new RelationshipUpdate
        {
            Relationship = new Relationship(
                x.Resource.EnsurePrefix(_prefix), x.Relation, x.Subject.EnsurePrefix(_prefix), EnsureCaveatIsPrefixed(x.OptionalCaveat)
            ),
            Operation = RelationshipUpdateOperation.Upsert
        }).ToList();

        return await WriteRelationshipsAsync(request);
    }

    /// <summary>
    /// Add or update a relationship
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public async Task<ZedToken> AddRelationshipAsync(Relationship relation)
    {
        return (await _spiceDbCore.Permissions.UpdateRelationshipAsync(EnsurePrefix(relation.Resource.Type)!,
                relation.Resource.Id, relation.Relation, EnsurePrefix(relation.Subject.Type)!, relation.Subject.Id,
                relation.Subject.Relation, caveat: EnsureCaveatIsPrefixed(relation.OptionalCaveat)))
            .ToSpiceDbToken()!;
    }

    public ZedToken AddRelationship(Relationship relation)
    {
        return AddRelationshipAsync(relation).Result;
    }

    public async Task<ZedToken> AddRelationshipAsync(string relation)
    {
        return await AddRelationshipAsync(new Relationship(relation));
    }

    public ZedToken AddRelationship(string relation)
    {
        return AddRelationshipAsync(new Relationship(relation)).Result;
    }

    /// <summary>
    /// Removes an existing relationship (if it exists)
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public async Task<ZedToken> DeleteRelationshipAsync(Relationship relation)
    {
        return (await _spiceDbCore.Permissions.UpdateRelationshipAsync(EnsurePrefix(relation.Resource.Type)!,
            relation.Resource.Id, relation.Relation, EnsurePrefix(relation.Subject.Type)!, relation.Subject.Id,
            relation.Subject.Relation, Authzed.Api.V1.RelationshipUpdate.Types.Operation.Delete)).ToSpiceDbToken()!;
    }

    /// <summary>
    /// Removes an existing relationship (if it exists)
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public async Task<ZedToken> DeleteRelationshipAsync(string relation)
    {
        return await DeleteRelationshipAsync(new Relationship(relation));
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
    public async IAsyncEnumerable<LookupSubjectsResponse> LookupSubjects(ResourceReference resource,
        string permission,
        string subjectType, string optionalSubjectRelation = "",
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        await foreach (var response in _spiceDbCore.Permissions.LookupSubjects(EnsurePrefix(resource.Type)!, resource.Id,
                           permission, EnsurePrefix(subjectType)!,
                           optionalSubjectRelation,
                           context, zedToken.ToAuthzedToken(), cacheFreshness))
            yield return response;
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
    public async IAsyncEnumerable<LookupResourcesResponse> LookupResources(string resourceType,
        string permission,
        ResourceReference subject,
        Dictionary<string, object>? context = null,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        await foreach (var response in _spiceDbCore.Permissions.LookupResources(EnsurePrefix(resourceType)!, permission,
                           EnsurePrefix(subject.Type)!, subject.Id, subject.Relation,
                           context, zedToken.ToAuthzedToken(), cacheFreshness))
            yield return response;
    }

    public async IAsyncEnumerable<WatchResponse> Watch(List<string>? optionalSubjectTypes = null,
        ZedToken? zedToken = null,
        DateTime? deadline = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in _spiceDbCore.Watch.Watch(optionalSubjectTypes?.Select(x => EnsurePrefix(x)!).ToList(),
                           zedToken.ToAuthzedToken(), deadline, cancellationToken))
            yield return response;
    }

    public async Task<List<string>> GetResourcePermissionsAsync(string resourceType, string permission,
        ResourceReference subject, ZedToken? zedToken = null,
        CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        return await _spiceDbCore.Permissions.GetResourcePermissionsAsync(EnsurePrefix(resourceType)!, permission,
            EnsurePrefix(subject.Type)!, subject.Id, zedToken.ToAuthzedToken());
    }

    public string ReadSchema()
    {
        return _spiceDbCore.Schema.ReadSchemaAsync().Result;
    }

    public async Task<string> ReadSchemaAsync()
    {
        return await _spiceDbCore.Schema.ReadSchemaAsync();
    }

    public async Task WriteSchemaAsync(string schema)
    {
        await _spiceDbCore.Schema.WriteSchemaAsync(schema);
    }

    public async Task ImportSchemaFromFileAsync(string filePath)
    {
        await ImportSchemaFromStringAsync(await File.ReadAllTextAsync(filePath));
    }

    /// <summary>
    /// Imports an Authzed Playground compatible schema (not a yaml file, just the commented schema)
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public async Task ImportSchemaFromStringAsync(string schema)
    {
        var prefix = _prefix;

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
                    relations.Add(relationship.Name, new List<string>());

                relations[relationship.Name.Trim()].Add(relationship.SubjectType.Trim());
            });

            foreach (var key in relations.Keys)
                def += $"\trelation {key}: " + string.Join(" | ", relations[key].Select(x => $"{prefix}{x}").ToList()) +
                       "\n";

            foreach (var permission in entity.Permissions)
                def += $"\tpermission {permission.Name} = {permission.Definition}\n";

            def += "}\n\n";

            parsedSchema += def;
        }

        await _spiceDbCore.Schema.WriteSchemaAsync(parsedSchema);
    }

    public async Task<ZedToken?> ImportRelationshipsFromFileAsync(string filePath)
    {
        return await ImportRelationshipsAsync(await File.ReadAllTextAsync(filePath));
    }

    public async Task<ZedToken?> ImportRelationshipsAsync(string content)
    {
        // Read the file as one string.
        var operation = Authzed.Api.V1.RelationshipUpdate.Types.Operation.Touch;
        RepeatedField<Authzed.Api.V1.RelationshipUpdate> updateCollection = new();

        var lines = content.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            string[] cols = Regex.Split(line.Trim(), ":|@|#"); //refer to authzed docs for separator meanings
            if (cols.Length == 5)
            {
                var updateItem = _spiceDbCore.Permissions.GetRelationshipUpdate(EnsurePrefix(cols[0])!, cols[1], cols[2],
                    EnsurePrefix(cols[3])!, cols[4], "", operation);
                _spiceDbCore.Permissions.UpdateRelationships(ref updateCollection, updateItem);
            }
            else if (cols.Length == 6) //contain an additional column of optional subject relation
            {
                var updateItem = _spiceDbCore.Permissions.GetRelationshipUpdate(EnsurePrefix(cols[0])!, cols[1], cols[2],
                    EnsurePrefix(cols[3])!, cols[4], cols[5], operation);
                _spiceDbCore.Permissions.UpdateRelationships(ref updateCollection, updateItem);
            }
        }

        return (await _spiceDbCore.Permissions.WriteRelationshipsAsync(updateCollection)).WrittenAt.ToSpiceDbToken();
    }

    /// <summary>
    /// CheckBulkPermissionsAsync issues a check on whether a subject has permission or is a member of a relation on a specific
    /// resource for each item in the list.
    /// The ordering of the items in the response is maintained in the response.Checks with the same subject/permission
    /// will automatically be batched for performance optimization.
    /// </summary>
    /// <param name="permissions"></param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(IEnumerable<string> permissions,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var items = permissions.Select(perm => new CheckBulkPermissionsRequestItem()
        { Permission = new Models.Permission(perm) });

        return await CheckBulkPermissionsAsync(items, zedToken, cacheFreshness);
    }

    /// <summary>
    /// CheckBulkPermissionsAsync issues a check on whether a subject has permission or is a member of a relation on a specific
    /// resource for each item in the list.
    /// The ordering of the items in the response is maintained in the response.Checks with the same subject/permission
    /// will automatically be batched for performance optimization.
    /// </summary>
    /// <param name="permissions"></param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(
        IEnumerable<Models.Permission> permissions,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var items = permissions.Select(perm => new CheckBulkPermissionsRequestItem() { Permission = perm });

        return await CheckBulkPermissionsAsync(items, zedToken, cacheFreshness);
    }

    /// <summary>
    /// CheckBulkPermissionsAsync issues a check on whether a subject has permission or is a member of a relation on a specific
    /// resource for each item in the list.
    /// The ordering of the items in the response is maintained in the response.Checks with the same subject/permission
    /// will automatically be batched for performance optimization.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="zedToken"></param>
    /// <param name="cacheFreshness"></param>
    /// <returns></returns>
    public async Task<CheckBulkPermissionsResponse?> CheckBulkPermissionsAsync(
        IEnumerable<CheckBulkPermissionsRequestItem> items,
        ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var converted = items.Select(x => new Authzed.Api.V1.CheckBulkPermissionsRequestItem()
        {
            Context = x.Context.ToStruct(),
            Permission = x.Permission?.Relation,
            Resource = new Authzed.Api.V1.ObjectReference
            { ObjectId = x.Permission?.Resource.Id, ObjectType = EnsurePrefix(x.Permission?.Resource.Type) },
            Subject = new Authzed.Api.V1.SubjectReference()
            {
                Object = new Authzed.Api.V1.ObjectReference()
                { ObjectId = x.Permission?.Subject.Id, ObjectType = EnsurePrefix(x.Permission?.Subject.Type) }
            }
        });

        return await _spiceDbCore.Permissions.CheckBulkPermissionsAsync(converted, zedToken.ToAuthzedToken(), cacheFreshness);
    }

    private string? EnsurePrefix(string? type)
    {
        if (string.IsNullOrEmpty(type)) return type;

        return type.StartsWith(_prefix + "/") ? type : $"{_prefix}/{type}";
    }

    private Caveat? EnsureCaveatIsPrefixed(Caveat? caveat)
    {
        if (caveat is null) return null;

        caveat.Name = EnsurePrefix(caveat.Name)!;

        return caveat;
    }
}
