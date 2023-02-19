using System.Reflection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
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

        _client.ImportSchemaFromStringAsync(_schema).GetAwaiter().GetResult();

        _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "group" }).GetAwaiter().GetResult();

        var results = Task.WhenAll(
	        new[]
	        {
		        _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "group" }),
		        _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "organization" }),
		        _client.DeleteRelationshipsAsync(new RelationshipFilter { Type = "platform" }),
            }
        ).Result;

        _client.ImportRelationshipsAsync(_relationships).GetAwaiter().GetResult();
	}

    // TODO: Implement all tests
    [Test]
    public void ReadRelationshipsAsyncTest()
    {
	    Assert.Pass();
    }

    /*
        [Test]
        public void ReadRelationshipsAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void WriteRelationshipsAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void DeleteRelationshipsAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void CheckPermissionAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ExpandPermissionAsyncTest()
        {
            Assert.Fail();
        }

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

        [Test]
        public void ReadSchemaTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ImportSchemaFromFileAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ImportSchemaFromStringAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ImportRelationshipsFromFileAsyncTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ImportRelationshipsAsyncTest()
        {
            Assert.Fail();
        }
    */
}