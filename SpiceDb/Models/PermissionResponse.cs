using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents the response from a permission check operation.
/// Contains the permissionship result and the token representing when the check was made.
/// </summary>
public class PermissionResponse
{
	/// <summary>
	/// Gets or sets the permissionship result.
	/// Maps to the Permissionship field of CheckPermissionResponse in the protobuf.
	/// </summary>
	public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;

	/// <summary>
	/// Gets or sets the token representing the state of the system when the check was performed.
	/// </summary>
	public ZedToken? ZedToken { get; set; }

	/// <summary>
	/// Gets a value indicating whether the subject has the requested permission.
	/// </summary>
	public bool HasPermission => Permissionship is Permissionship.HasPermission;
}
