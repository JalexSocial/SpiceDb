using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;

namespace SpiceDb.SchemaParsingTest;

public class CodeBuilder
{
	private readonly Parser _parser;

	public CodeBuilder (Parser parser)
	{
		_parser = parser;
	}

	public string BuildRelationshipCode()
	{
		var relationshipCode = new StringBuilder();
		var definitionCode = new Dictionary<string, StringBuilder>();
		
		foreach (var definition in _parser.Schema.Definitions)
		{
			definitionCode.TryAdd(definition.Name, new());
		}

		foreach (var definition in _parser.Schema.Definitions)
		{
			foreach (var relation in definition.Relations)
			{
				foreach (var relType in relation.Types)
				{
					//relmap.TryAdd(typ.Type)
					//Console.Write($"   - {relType.Type}");

					//if (!string.IsNullOrEmpty(relType.Relation))
					//	Console.Write($" (with subject relation {relType.Relation})");

					//Console.WriteLine();

					if (!string.IsNullOrEmpty(relType.Relation))
					{
						definitionCode[definition.Name].AppendLine($"    public Relationship Relate{relation.Name.Pascalize()}(Zed{relType.Type.Pascalize()} subject) => new Relationship(this, \"{relation.Name}\", subject.WithSubjectRelation(\"{relType.Relation}\"));");
					}
					else
					{
						definitionCode[definition.Name].AppendLine($"    public Relationship Relate{relation.Name.Pascalize()}(Zed{relType.Type.Pascalize()} subject) => new Relationship(this, \"{relation.Name}\", subject);");
					}
				}
			}

			List<string> permissions = definition.Permissions.Select(x => x.Name).ToList();

			for (int k = 0; k < permissions.Count; k++)  
			{
				var permission = permissions[k];
				var key = !permission.Contains("#") ? $"{definition.Name}#{permission}" : permission;

				if (_parser.PermissionMap.ContainsKey(key))
				{
					//  public SpiceDb.Models.Permission OrgIsAdministrator(ZedOrganization resource) => new SpiceDb.Models.Permission(resource, "admin", this);
					List<string> pmap = new(_parser.PermissionMap[key]);
					HashSet<string> subjects = new();

					for (int i = 0; i < pmap.Count; i++)
					{
						var relation = pmap[i];

						// relation is either a true relation or another permission
						if (_parser.RelationshipMap.ContainsKey(relation))
						{
							foreach (var prel in _parser.RelationshipMap[relation])
								subjects.Add(prel);
						}
						else
						{
							if (relation.Contains("->")) // Skip synthetic permissions for a while
							{
								var prel = relation.Replace("->", "#");
								if (!permissions.Contains(prel))
									permissions.Add(prel);
								continue;
							}

							pmap.AddRange(new List<string>(_parser.PermissionMap[relation]).Except(pmap));
						}
					}

					foreach (var subject in subjects)
					{
						definitionCode[subject].AppendLine($"    public SpiceDb.Models.Permission {definition.Name.Pascalize()}{permission.Pascalize()}(Zed{definition.Name.Pascalize()} resource) => new SpiceDb.Models.Permission(resource, \"{permission}\", this);");
					}
				}

			}

		}

		foreach (var key in definitionCode.Keys)
		{
			Console.WriteLine($"public class Zed{key.Pascalize()} " + "{\n" + definitionCode[key].ToString() + "}\n");
			relationshipCode.Append(definitionCode[key].ToString());
		}

		return relationshipCode.ToString();
	}
}
