using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpiceDb.Enum;
using SpiceDb.Models;
using System.Reflection;

namespace SpiceDb.Tests;

[TestFixture]
public class SpiceDbClientTests
{
    private SpiceDbClient? _client;
    private string _prefix = "client";
    private string _schema = string.Empty;
    private string _relationships = string.Empty;

    [OneTimeSetUp]
    public void SetUp()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var builder = new ConfigurationBuilder()
            .AddUserSecrets(assembly)
            .AddEnvironmentVariables();
        var configurationRoot = builder.Build();

        // spicedb serve-testing
        // default server address: 127.0.0.1:50051
        var serverAddress = configurationRoot.GetValue<string>("SERVER_ADDRESS");
        var token = configurationRoot.GetValue<string>("TOKEN") ?? string.Empty;

        if (string.IsNullOrEmpty(serverAddress))
        {
            ClassicAssert.Fail("Unable to load service configuration from environment variables");
        }

        _client = new SpiceDbClient(serverAddress!, token!, _prefix);
        _schema = assembly.ReadResourceAsync("schema.txt").Result;
        _relationships = assembly.ReadResourceAsync("relationships.txt").Result;

        // Import the new schema
        _client.ImportSchemaFromStringAsync(_schema).GetAwaiter().GetResult();

        // Delete any existing relationships
        var results = Task.WhenAll(
            new[]
            {
                _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "group" }),
                _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "organization" }),
                _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "platform" }),
            }
        ).Result;

        // Import relationships to set up test
        _client.ImportRelationshipsAsync(_relationships).GetAwaiter().GetResult();
    }

    private List<string> GetRelationships(string type)
    {
        return _relationships.Split("\n").Select(s => s.Trim()).Where(s => s.StartsWith(type)).ToList();
    }

    // TODO: Implement all tests
    [Test]
    public async Task ReadRelationshipsAsyncTest_FilterOnly()
    {
        var expected = GetRelationships("group");
        List<string> relationships = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group" }, excludePrefix: true))
        {
            relationships.Add(response.Relationship.ToString()!);
        }

        foreach (var relationship in expected)
        {
            ClassicAssert.IsTrue(relationships.Contains(relationship));
        }
    }

    [Test]
    public async Task ReadRelationshipsAsyncTest_FilterSubjectType()
    {
        var expected = GetRelationships("group:security");
        List<string> relationships = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group" }, new SubjectFilter { Type = "user", OptionalId = "jimmy" }, excludePrefix: true))
        {
            relationships.Add(response.Relationship.ToString()!);
        }

        foreach (var relationship in expected)
        {
            ClassicAssert.IsTrue(relationships.Contains(relationship));
        }
    }

    [Test]
    public async Task WriteAndDeleteRelationshipsAsyncTest_Upsert()
    {
        List<RelationshipUpdate> updates = new();
        updates.Add(new RelationshipUpdate
        {
            Operation = RelationshipUpdateOperation.Upsert,
            Relationship = new Relationship("group:security#owner@user:bart")
        });

        await _client!.WriteRelationshipsAsync(updates);

        var hasRelationship = _client!.CheckPermission("group:security#owner@user:bart");

        await _client!.DeleteRelationshipAsync(new Relationship("group:security#owner@user:bart"));

        var stillHasRelationship = _client!.CheckPermission("group:security#owner@user:bart");

        ClassicAssert.IsTrue(hasRelationship.HasPermission && !stillHasRelationship.HasPermission);
    }

    [Test]
    public async Task DeleteRelationshipsAsyncTest()
    {
        _client!.AddRelationship("group:delete#manager@user:test1");
        _client!.AddRelationship("group:delete#manager@user:test2");
        _client!.AddRelationship("group:delete#manager@user:test3");
        _client!.AddRelationship("group:delete#manager@user:test4");

        List<string> relationships = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete" }, new SubjectFilter { Type = "user" }, excludePrefix: true))
        {
            relationships.Add(response.Relationship.ToString()!);
        }

        await _client.DeleteRelationshipsAsync(new RelationshipFilter
        {
            Type = "group",
            OptionalId = "delete",
            OptionalRelation = "manager"
        },
            new SubjectFilter
            {
                Type = "user"
            });

        List<string> relationships2 = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete" }, new SubjectFilter { Type = "user" }, excludePrefix: true))
        {
            relationships2.Add(response.Relationship.ToString()!);
        }

        ClassicAssert.IsTrue(relationships.Count == 4 && relationships2.Count == 0);
    }

    [Test]
    public async Task DeleteRelationshipsAsyncWithSubjectIdTest()
    {
        _client!.AddRelationship("group:delete#manager@user:test1");
        _client!.AddRelationship("group:delete#manager@user:test2");
        _client!.AddRelationship("group:delete#manager@user:test3");
        _client!.AddRelationship("group:delete#manager@user:test4");

        List<string> relationships = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete" }, new SubjectFilter { Type = "user" }, excludePrefix: true))
        {
            relationships.Add(response.Relationship.ToString()!);
        }

        await _client.DeleteRelationshipsAsync(new RelationshipFilter
        {
            Type = "group",
            OptionalId = "delete",
            OptionalRelation = "manager"
        },
            new SubjectFilter
            {
                Type = "user",
                OptionalId = "test1"
            });

        List<string> relationships2 = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete" }, new SubjectFilter { Type = "user" }, excludePrefix: true))
        {
            relationships2.Add(response.Relationship.ToString()!);
        }

        ClassicAssert.IsTrue(relationships.Count == 4 && relationships2.Count == 3);
    }

    [Test]
    public async Task AddRelationshipsAsync_AddBatchRelationships_ReturnsValidToken()
    {
        // Arrange: Create a batch of new relationships to add
        var relationships = new List<Relationship>
        {
            new Relationship("group:devGroup", "direct_member", "user:charlie"),
            new Relationship("group:devGroup", "owner", "user:dave")
        };

        // Act: Add these relationships using AddRelationshipsAsync
        var resultToken = await _client!.AddRelationshipsAsync(relationships);

        // Assert: Check that a valid ZedToken is returned, indicating success
        ClassicAssert.IsNotNull(resultToken);
        ClassicAssert.IsNotEmpty(resultToken!.Token);
    }

    [Test]
    public void CheckPermissionAsyncTest()
    {
        var p1 = _client!.CheckPermission("organization:authzed#member@user:jake");
        var p2 = _client!.CheckPermission("group:test#viewers@user:jake");
        var p3 = _client!.CheckPermission("organization:authzed#admin@user:michael");
        var p4 = _client!.CheckPermission("group:test#posters@user:somenewguy");
        var p5 = _client!.CheckPermission("group:test#joiners@user:somenewguy");
        var p6 = _client!.CheckPermission("group:test#add_manager@user:blackhat");

        ClassicAssert.IsTrue(p1.HasPermission && p2.HasPermission && p3.HasPermission && p4.HasPermission && !p5.HasPermission && !p6.HasPermission);
    }

    /// This feature isn't currently available in Serverless
    [Test]
    public async Task CheckBulkPermissionAsyncTest()
    {
        var permissions = new[]
        {
            "organization:authzed#member@user:jake",
            "group:test#viewers@user:jake",
            "organization:authzed#admin@user:michael",
            "group:test#posters@user:somenewguy",
            "group:test#joiners@user:somenewguy",
            "group:test#add_manager@user:blackhat"
        };

        var p = await _client!.CheckBulkPermissionsAsync(permissions);

        ClassicAssert.IsNotNull(p);
        ClassicAssert.IsTrue(p!.Pairs[0].HasPermission && p!.Pairs[1].HasPermission && p!.Pairs[2].HasPermission && p!.Pairs[3].HasPermission && !p!.Pairs[4].HasPermission && !p!.Pairs[5].HasPermission);
    }

    [Test]
    public async Task ExpandPermissionAsyncTest()
    {
        var response = await _client!.ExpandPermissionAsync(new ResourceReference("group", "test"), "post");

        ClassicAssert.IsNotNull(response);
    }

    [Test]
    public async Task LookupResources_UserCanViewPosts_ReturnsAllGroups()
    {
        // Arrange: Define a subject with permission to view posts
        var subject = new ResourceReference("user", "jake");
        string permission = "view_posts";

        // Act: Lookup all resources (groups) where 'user:jake' can view posts
        var accessibleResources = new List<string>();
        await foreach (var resource in _client!.LookupResources("group", permission, subject))
        {
            accessibleResources.Add($"group:{resource.ResourceId}");
        }

        // Assert: Check that 'user:jake' can view posts in the 'test' group and potentially others
        var expectedResources = new List<string> { "group:test" }; // Adjust as needed based on detailed schema analysis
        CollectionAssert.AreEquivalent(expectedResources, accessibleResources);
    }

    [Test]
    public async Task LookupSubjects_GroupTestMembers_ReturnsExpectedMembers()
    {
        // Arrange: Specify the resource and the permission to check for membership
        var resource = new ResourceReference("group", "test");
        string permission = "member";

        // Act: Lookup subjects who are members of 'group:test'
        var members = new List<string>();
        await foreach (var subjectResponse in _client!.LookupSubjects(resource, permission, "user"))
        {
            members.Add($"user:{subjectResponse.Subject.Id}");
        }

        // Assert: Verify that the expected subjects are returned
        var expectedSubjects = new List<string> { "user:jake", "user:jimmy", "user:joey" }; // Include users who have 'member' or higher access
        CollectionAssert.AreEquivalent(expectedSubjects, members);
    }

    /*    
[Test]
public void AddRelationshipAsyncTest()
{
ClassicAssert.Fail();
}

[Test]
public void DeleteRelationshipAsyncTest()
{
ClassicAssert.Fail();
}

[Test]
public void LookupSubjectsTest()
{
ClassicAssert.Fail();
}

[Test]
public void LookupResourcesTest()
{
ClassicAssert.Fail();
}

[Test]
public void WatchTest()
{
ClassicAssert.Fail();
}

[Test]
public void GetResourcePermissionsAsyncTest()
{
ClassicAssert.Fail();
}
*/
    [Test]
    public void ReadSchemaTest()
    {
        var schema = _client!.ReadSchema();

        ClassicAssert.IsFalse(string.IsNullOrEmpty(schema));
    }


}