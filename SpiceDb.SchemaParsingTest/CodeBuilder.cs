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
			Dictionary<string, RelationshipCodeDefinition> codeDef = new();

			foreach (var relation in definition.Relations)
			{
				foreach (var relType in relation.Types)
				{
					//relmap.TryAdd(typ.Type)
					//Console.Write($"   - {relType.Type}");

					//if (!string.IsNullOrEmpty(relType.Relation))
					//	Console.Write($" (with subject relation {relType.Relation})");

					//Console.WriteLine();
					var relationshipDef = new RelationshipCodeDefinition
					{
						MethodName = $"Relate{relation.Name.Pascalize()}",
						SubjectName = $"Zed{relType.Type.Pascalize()}",
						Relation = relation.Name
					};

					var key = $"{relationshipDef.MethodName}:{relationshipDef.SubjectName}";
					codeDef.TryAdd(key, relationshipDef);

					if (!string.IsNullOrEmpty(relType.Relation))
					{
						codeDef[key].OptionalRelations.Add(relType.Relation);
					}

					// How to add to codeDef?
				}
			}

			foreach (var key in codeDef.Keys)
			{
				var relationshipDef = codeDef[key];

				if (relationshipDef.OptionalRelations.Count > 0)
				{
					foreach (var optionalRelation in relationshipDef.OptionalRelations)
					{
						var relText = optionalRelation == "*" ? "All" : optionalRelation.Pascalize();
						definitionCode[definition.Name].AppendLine($"    public Relationship {relationshipDef.MethodName}{relText}({relationshipDef.SubjectName} subject) => new Relationship(this, \"{relationshipDef.Relation}\", subject.WithSubjectRelation(\"{optionalRelation}\"));");
					}
				}
				else
				{
					definitionCode[definition.Name].AppendLine($"    public Relationship {relationshipDef.MethodName}({relationshipDef.SubjectName} subject) => new Relationship(this, \"{relationshipDef.Relation}\", subject);");
				}
			}

			List<string> permissions = definition.Permissions.Select(x => x.Name).ToList();

			for (int k = 0; k < permissions.Count; k++)  
			{
				var permission = permissions[k];
				var key = !permission.Contains("#") ? $"{definition.Name}#{permission}" : permission;

				//  public SpiceDb.Models.Permission OrgIsAdministrator(ZedOrganization resource) => new SpiceDb.Models.Permission(resource, "admin", this);
				var subjects = ResolvePermissionSubjects(key);

				foreach (var subject in subjects)
				{
					definitionCode[subject].AppendLine($"    public SpiceDb.Models.Permission {definition.Name.Pascalize()}{permission.Pascalize()}(Zed{definition.Name.Pascalize()} resource) => new SpiceDb.Models.Permission(resource, \"{permission}\", this);");
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

	/// <summary>
	/// Format of permission is definition#permission_name
	/// </summary>
	/// <param name="permission"></param>
	/// <returns></returns>
	private HashSet<string> ResolvePermissionSubjects(string permission)
	{
		HashSet<string> subjects = new();

		if (_parser.PermissionMap.ContainsKey(permission))
		{
			List<string> pmap = new(_parser.PermissionMap[permission]);

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

						var otherSubjects = ResolvePermissionSubjects(prel);

						foreach (var otherSubject in otherSubjects)
						{
							subjects.Add(otherSubject);
						}

						continue;
					}

					pmap.AddRange(new List<string>(_parser.PermissionMap[relation]).Except(pmap));
				}
			}
		}

		return subjects;
	}
}

internal class RelationshipCodeDefinition
{
	public string MethodName { get; set; } = string.Empty;
	public string SubjectName { get; set; } = string.Empty;
	public string Relation { get; set; } = string.Empty;
	public List<string> OptionalRelations { get; set; } = new();
}
