using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.SchemaParsingTest;

public class Parser
{
	private readonly Schema _schema;

	public Parser(Schema schema)
	{
		_schema = schema;
	}

	public HashSet<string> GetTerminalTypeNames()
	{
		var terminalTypes = new HashSet<string>();
		foreach (var definition in _schema.Definitions)
		{
			terminalTypes.Add(definition.Name);
		}
		return terminalTypes;
	}

	private void ResolveType(string typeName, HashSet<string> resolvedTypes, HashSet<string> terminalTypes)
	{
		if (terminalTypes.Contains(typeName))
		{
			resolvedTypes.Add(typeName);
		}
		else
		{
			Definition? definition;
			if (typeName.Contains("#"))
				definition = _schema.Definitions.FirstOrDefault(d => d.Name == typeName.Split("#").First());
			else
				definition = _schema.Definitions.FirstOrDefault(d => d.Name == typeName);

			if (definition != null)
			{
				var relation = definition.Relations.FirstOrDefault(r => r.Name == typeName.Split("#").Last());

				if (relation != null)
				{
					foreach (var type in relation.Types)
					{
						var rtype = type.Type;
						if (!string.IsNullOrEmpty(type.Relation))
							rtype = $"{type.Type}#{type.Relation}";

						ResolveType(rtype, resolvedTypes, terminalTypes);
					}
				}
			}
		}
	}

	public Dictionary<string, HashSet<string>> BuildRelationshipMap()
	{
		var map = new Dictionary<string, HashSet<string>>();
		var terminalTypes = GetTerminalTypeNames();

		foreach (var definition in _schema.Definitions)
		{
			foreach (var relation in definition.Relations)
			{
				foreach (var type in relation.Types)
				{
					var key = $"{definition.Name}#{relation.Name}";
					if (!map.ContainsKey(key))
						map[key] = new HashSet<string>();

					var rtype = type.Type;
					if (!string.IsNullOrEmpty(type.Relation))
						rtype = $"{type.Type}#{type.Relation}";

					ResolveType(rtype, map[key], terminalTypes);
				}
			}
		}

		return map;
	}
}
