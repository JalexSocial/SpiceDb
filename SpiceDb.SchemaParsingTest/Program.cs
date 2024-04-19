// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using Humanizer;
using SpiceDb.SchemaParsingTest;

Console.WriteLine("Hello, World!");

var schemaText = File.ReadAllText("schema.zedj");
var schema = JsonSerializer.Deserialize<Schema>(schemaText);

var parser = new Parser(schema!);
var terminals = parser.GetTerminalTypeNames();
var types = parser.BuildRelationshipMap();

/*
var relmap = new Dictionary<string, string>();

if (schema != null)
{
	foreach (var definition in schema.Definitions)
	{
		var relationshipCode = new StringBuilder();

		Console.WriteLine(definition.Name);
		relmap.TryAdd(definition.Name, string.Empty);

		foreach (var relation in definition.Relations)
		{
			Console.WriteLine($" - " + relation.Name);
			foreach (var relType in relation.Types)
			{
				//relmap.TryAdd(typ.Type)
				Console.Write($"   - {relType.Type}");
				
				if (!string.IsNullOrEmpty(relType.Relation))
					Console.Write($" (with subject relation {relType.Relation})");

				Console.WriteLine();

				if (!string.IsNullOrEmpty(relType.Relation))
				{
					relationshipCode.AppendLine($"public Relationship Relate{relation.Name.Pascalize()}(Zed{relType.Type.Pascalize()} subject) => new Relationship(this, \"{relation.Name}\", subject.WithSubjectRelation(\"{relType.Relation}\");");
				}
				else
				{
					relationshipCode.AppendLine($"public Relationship Relate{relation.Name.Pascalize()}(Zed{relType.Type.Pascalize()} subject) => new Relationship(this, \"{relation.Name}\", subject);");
				}
			}
		}

		foreach (var permission in definition.Permissions)
		{
			Console.WriteLine($" @ {permission.Name}");
		}

		var output = relationshipCode.ToString();
		Console.WriteLine(output);
	}
}
*/
Console.WriteLine();
