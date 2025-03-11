namespace SpiceDb.Models;

/// <summary>
/// Represents a permission definition as specified in the schema.
/// Contains the permission name and the expression that defines it.
/// </summary>
public class PermissionDefinition
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PermissionDefinition"/> class.
	/// </summary>
	/// <param name="name">The name of the permission.</param>
	/// <param name="definition">The definition or computation expression for the permission.</param>
	public PermissionDefinition(string name, string definition)
	{
		Name = name;
		Definition = definition;
	}

	/// <summary>
	/// Gets or sets the name of the permission.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the definition (expression) for the permission.
	/// </summary>
	public string Definition { get; set; }
}
