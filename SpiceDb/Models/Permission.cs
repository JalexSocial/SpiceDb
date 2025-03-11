namespace SpiceDb.Models;


public class Permission : Relationship
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Permission"/> class using resource, relation, and subject references.
	/// </summary>
	/// <param name="resource">The resource reference.</param>
	/// <param name="relation">The permission or relation name.</param>
	/// <param name="subject">The subject reference.</param>
	public Permission(ResourceReference resource, string relation, ResourceReference subject) : base(resource, relation, subject)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="Permission"/> class using string representations.
	/// </summary>
	/// <param name="resource">The resource identifier.</param>
	/// <param name="relation">The permission or relation name.</param>
	/// <param name="subject">The subject identifier.</param>
	public Permission(string resource, string relation, string subject) : base(resource, relation, subject)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Permission"/> class from a permission string.
	/// The string must be in the format "resource#relation@subject".
    /// Example: user:1234#view@user:1 (can user:1 view user:1234?)
	/// </summary>
	/// <param name="permission">The permission string.</param>
	/// <exception cref="ArgumentException">Thrown if the string is not in the expected format.</exception>
	public Permission(string permission) : base(permission)
	{
	}
}
