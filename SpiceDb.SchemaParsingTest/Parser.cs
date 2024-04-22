using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.SchemaParsingTest;

public class Parser
{
	private readonly Schema _schema;
	private readonly HashSet<string> _terminalTypes;
	private readonly Dictionary<string, HashSet<string>> _relationshipMap = new();
	private readonly Dictionary<string, HashSet<string>> _permissionMap = new();

	public Parser(Schema schema)
	{
		_schema = schema;
		_terminalTypes = GetTerminalTypeNames();

		BuildPermissionMap();
		BuildRelationshipMap();
	}

	public Dictionary<string, HashSet<string>> RelationshipMap => _relationshipMap;
	public Dictionary<string, HashSet<string>> PermissionMap => _permissionMap;

	public Schema Schema => _schema;

	private HashSet<string> GetTerminalTypeNames()
	{
		var terminalTypes = new HashSet<string>();
		foreach (var definition in _schema.Definitions)
		{
			terminalTypes.Add(definition.Name);
		}
		return terminalTypes;
	}

	private void ResolveType(string typeName, HashSet<string> resolvedTypes, HashSet<string>? walkedPermissions = null)
	{
		walkedPermissions ??= new();

		if (_terminalTypes.Contains(typeName))
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

						ResolveType(rtype, resolvedTypes, walkedPermissions);
					}
				}
				else
				{
					// Unable to resolve this as a relation so it's probably a permission
					if (_permissionMap.ContainsKey(typeName) && !walkedPermissions.Contains(typeName))
					{
						var permRels = _permissionMap[typeName];

						foreach (var rel in permRels)
						{
							walkedPermissions.Add(typeName);
							ResolveType(rel.Replace("->", "#"), resolvedTypes, walkedPermissions);
						}
					}
				}
			}
		}
	}

	private void ResolvePermissionRelationships(string definitionName, UserSet? userset, HashSet<string> relations)
	{
		if (userset is null)
			return;

		foreach (var child in userset.Children)
		{
			if (!string.IsNullOrEmpty(child.Permission))
			{
				// This is a synthetic relationship so subjects will come from looking up another permission
				relations.Add($"{child.Relation}->{child.Permission}");
			}
			else if (!string.IsNullOrEmpty(child.Relation))
				relations.Add($"{definitionName}#{child.Relation}");

			if (child.Children.Count > 0)
				ResolvePermissionRelationships(definitionName, child, relations);
		}
	}

	private void BuildPermissionMap()
	{
		foreach (var definition in _schema.Definitions)
		{
			foreach (var permission in definition.Permissions)
			{
				var key = $"{definition.Name}#{permission.Name}";
				if (!_permissionMap.ContainsKey(key))
					_permissionMap[key] = new HashSet<string>();

				ResolvePermissionRelationships(definition.Name, permission.UserSet, _permissionMap[key]);
			}
		}
	}

	private void BuildRelationshipMap()
	{
		foreach (var definition in _schema.Definitions)
		{
			foreach (var relation in definition.Relations)
			{
				foreach (var type in relation.Types)
				{
					var key = $"{definition.Name}#{relation.Name}";
					if (!_relationshipMap.ContainsKey(key))
						_relationshipMap[key] = new HashSet<string>();

					var rtype = type.Type;
					if (!string.IsNullOrEmpty(type.Relation) && type.Relation != "*")
						rtype = $"{type.Type}#{type.Relation}";

					ResolveType(rtype, _relationshipMap[key]);
				}
			}
		}
	}
}
