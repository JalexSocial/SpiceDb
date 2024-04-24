using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpiceDb.SchemaParsingTest;

public class Schema
{
	[JsonPropertyName("definitions")] 
	public List<Definition> Definitions { get; set; } = new();
}

public class Definition
{
	[JsonPropertyName("name")] 
	public string Name { get; set; }
	[JsonPropertyName("namespace")] 
	public string? Namespace { get; set; }
	[JsonPropertyName("relations")] 
	public List<Relation> Relations { get; set; } = new();
	[JsonPropertyName("permissions")] 
	public List<Permission> Permissions { get; set; } = new();
	[JsonPropertyName("comment")] 
	public string? Comment { get; set; }
}

public class Relation
{
	[JsonPropertyName("name")] 
	public string Name { get; set; } = string.Empty;
	[JsonPropertyName("types")] 
	public List<RelationType> Types { get; set; } = new();
	[JsonPropertyName("comment")] 
	public string? Comment { get; set; }
}

public class RelationType
{
	[JsonPropertyName("type")] 
	public string Type { get; set; } = string.Empty;
	[JsonPropertyName("relation")] 
	public string? Relation { get; set; }
	[JsonPropertyName("comment")] 
	public string? Comment { get; set; }
}

public class Permission
{
	[JsonPropertyName("name")] 
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("userSet")] 
	public UserSet UserSet { get; set; } = default!;
	[JsonPropertyName("comment")] 
	public string? Comment { get; set; }
}

public class UserSet
{
	[JsonPropertyName("operation")] 
	public string? Operation { get; set; }
	[JsonPropertyName("relation")] 
	public string? Relation { get; set; }
	[JsonPropertyName("permission")] 
	public string? Permission { get; set; }
	[JsonPropertyName("children")] 
	public List<UserSet> Children { get; set; } = new();
}