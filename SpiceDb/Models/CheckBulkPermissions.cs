using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents the result of an individual permission check within a bulk permission request.
/// This corresponds to the pairing of request and response in the CheckBulkPermissionsResponse protobuf.
/// </summary>
public class CheckBulkPermissions
{
	/// <summary>
	/// Gets or sets the permission that was checked.
	/// </summary>
	public Permission Permission { get; set; } = default!;

	/// <summary>
	/// Gets or sets the context that was used during the permission check.
	/// </summary>
	public Dictionary<string, object> Context { get; set; } = new();

	/// <summary>
	/// Gets or sets the result of the permission check.
	/// Maps to the Permissionship enum in the protobuf.
	/// </summary>
	public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;

    /// <summary>
    /// Returns true if Permissionship is HasPermission
    /// </summary>
	public bool HasPermission => Permissionship is Permissionship.HasPermission;

	/// <summary>
	/// Gets or sets additional caveat information if the permission was only partially evaluated.
	/// </summary>
	public PartialCaveatInfo? PartialCaveatInfo { get; set; }

	/// <summary>
	/// Gets or sets any error information returned during the permission check.
	/// </summary>
	public Status? Error { get; set; }
}