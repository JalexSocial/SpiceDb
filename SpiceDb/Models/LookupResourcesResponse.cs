using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents a single resource found during a lookup operation.
/// Contains the lookup token, the resource identifier, and the permission result.
/// </summary>
public class LookupResourcesResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the system when the resource was looked up.
	/// </summary>
	public ZedToken? LookedUpAt { get; set; }

	/// <summary>
	/// Gets or sets the identifier of the found resource.
	/// </summary>
	public string ResourceId { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the permissionship result indicating whether the subject has permission on the resource.
	/// </summary>
	public SpiceDb.Enum.Permissionship Permissionship { get; set; } = Permissionship.Unspecified;

	/// <summary>
	/// Gets or sets any missing context fields if the response was partially evaluated.
	/// </summary>
	public List<string> MissingRequiredContext { get; set; } = new();
}
