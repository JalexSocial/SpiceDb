using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents a single permission check response item from a bulk permission check.
/// Corresponds to the CheckBulkPermissionsResponseItem message in the protobuf.
/// </summary>
public class CheckBulkPermissionsResponseItem
{
	/// <summary>
	/// Gets or sets the permissionship result.
	/// </summary>
	public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;

	/// <summary>
	/// Gets or sets additional caveat information if the permission check was partially evaluated.
	/// </summary>
	public PartialCaveatInfo? PartialCaveatInfo { get; set; }
}