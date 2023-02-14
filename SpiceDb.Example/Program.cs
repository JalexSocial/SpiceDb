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
var client = new SpiceDbClient(secrets.ServerAddress, secrets.Token);

// Add relationship where user:bob is a reader of document:firstdoc
client.AddRelation("arch/document:firstdoc#reader@arch/user:bob");

// Second approach to adding relationships
client.AddRelation(new Relationship("arch/document:firstdoc", "reader", "arch/user:jacob"));

// This approach uses a little syntactic sugar to define each of the relations
client.AddRelation(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

// Check to see if user:bob is in fact now a reader of document:firstdoc
var bobCanRead = client.CheckPermission(new Permission("arch/document:firstdoc#reader@arch/user:bob"));

Console.WriteLine($"Can user bob read document:firstdoc? {bobCanRead.HasPermission}");
// true

// Check to see if user:carmella is in fact now a reader of document:firstdoc
var carmellaCanRead = client.CheckPermission(ZedUser.WithId("carmella").CanRead(ZedDocument.WithId("firstdoc")));

Console.WriteLine($"Can user carmella read document:firstdoc? {carmellaCanRead.HasPermission}");
// true


