using Microsoft.Extensions.Configuration;
using SpiceDb.Example;
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
var client = new SpiceDb.Client(secrets.ServerAddress, secrets.Token);

// Add relationship where user:bob is a reader of document:firstdoc
client.AddRelation("arch/document:firstdoc#reader@arch/user:bob");

// Check to see if user:bob is in fact now a reader of document:firstdoc
var canRead = client.CheckPermission(new Permission("arch/document:firstdoc#reader@arch/user:bob"));

Console.WriteLine($"Can user bob read document:firstdoc? {canRead}");




