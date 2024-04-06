using System.Reflection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SpiceDb.Enum;
using SpiceDb.Models;

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

        var serverAddress = configurationRoot.GetValue<string>("SERVER_ADDRESS");
		var token = configurationRoot.GetValue<string>("TOKEN");

		if (string.IsNullOrEmpty(serverAddress) || string.IsNullOrEmpty(token))
		{
            Assert.Fail("Unable to load service configuration from environment variables");
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
	        Assert.IsTrue(relationships.Contains(relationship));
        }
    }

    [Test]
    public async Task ReadRelationshipsAsyncTest_FilterSubjectType()
    {
	    var expected = GetRelationships("group:security");
	    List<string> relationships = new();

	    await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group" }, new RelationshipFilter { Type = "user", OptionalId = "jimmy"}, excludePrefix: true))
	    {
		    relationships.Add(response.Relationship.ToString()!);
	    }

	    foreach (var relationship in expected)
	    {
		    Assert.IsTrue(relationships.Contains(relationship));
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

        Assert.IsTrue(hasRelationship.HasPermission && !stillHasRelationship.HasPermission);
    }

    [Test]
    public async Task DeleteRelationshipsAsyncTest()
    {
        _client!.AddRelationship("group:delete#manager@user:test1");
        _client!.AddRelationship("group:delete#manager@user:test2");
        _client!.AddRelationship("group:delete#manager@user:test3");
        _client!.AddRelationship("group:delete#manager@user:test4");

        List<string> relationships = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete" }, new RelationshipFilter { Type = "user" }, excludePrefix: true))
        {
	        relationships.Add(response.Relationship.ToString()!);
        }

        await _client.DeleteRelationshipsAsync(new RelationshipFilter
	        {
		        Type = "group",
		        OptionalId = "delete",
		        OptionalRelation = "manager"
	        },
	        new RelationshipFilter
	        {
		        Type = "user"
	        });

        List<string> relationships2 = new();

        await foreach (var response in _client!.ReadRelationshipsAsync(new RelationshipFilter { Type = "group", OptionalId = "delete"}, new RelationshipFilter { Type = "user" }, excludePrefix: true))
        {
	        relationships2.Add(response.Relationship.ToString()!);
        }

        Assert.IsTrue(relationships.Count == 4 && relationships2.Count == 0);
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

        Assert.IsTrue(p1.HasPermission && p2.HasPermission && p3.HasPermission && p4.HasPermission && !p5.HasPermission && !p6.HasPermission);
    }

    [Test]
    public async Task BulkCheckPermissionAsyncTest()
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

	    var p = await _client!.BulkCheckPermissionAsync(permissions);

        Assert.IsNotNull(p);
	    Assert.IsTrue(p!.Pairs[0].HasPermission && p!.Pairs[1].HasPermission && p!.Pairs[2].HasPermission && p!.Pairs[3].HasPermission && !p!.Pairs[4].HasPermission && !p!.Pairs[5].HasPermission);
    }

	[Test]
    public async Task ExpandPermissionAsyncTest()
    {
	    var response = await _client!.ExpandPermissionAsync(new ResourceReference("group", "test"), "post");

        Assert.IsNotNull(response);
    }
            /*    
   [Test]
   public void AddRelationshipsAsyncTest()
   {
       Assert.Fail();
   }

   [Test]
   public void AddRelationshipAsyncTest()
   {
       Assert.Fail();
   }

   [Test]
   public void DeleteRelationshipAsyncTest()
   {
       Assert.Fail();
   }

   [Test]
   public void LookupSubjectsTest()
   {
       Assert.Fail();
   }

   [Test]
   public void LookupResourcesTest()
   {
       Assert.Fail();
   }

   [Test]
   public void WatchTest()
   {
       Assert.Fail();
   }

   [Test]
   public void GetResourcePermissionsAsyncTest()
   {
       Assert.Fail();
   }
*/
    [Test]
    public void ReadSchemaTest()
    {
	    var schema = _client!.ReadSchema();

        Assert.IsFalse(string.IsNullOrEmpty(schema));
    }
    

}