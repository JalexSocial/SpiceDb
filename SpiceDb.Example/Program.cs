using Microsoft.Extensions.Configuration;
using SpiceDb;
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
var kevinCanRead = client.CheckPermission("document:firstdoc#reader@user:bob");

Console.WriteLine($"Can user kevin read document:firstdoc? {kevinCanRead.HasPermission}");
// true


// Check to see if user:carmella is in fact now a reader of document:firstdoc
var carmellaCanRead = client.CheckPermission(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

Console.WriteLine($"Can user carmella read document:firstdoc? {carmellaCanRead.HasPermission}");
// true

var subjectPageable = client.LookupSubjects(
	new ResourceReference("document", "firstdoc"),
	"reader",
	"user");

await foreach (var subject in subjectPageable)
{
	// do anything or nothing
}