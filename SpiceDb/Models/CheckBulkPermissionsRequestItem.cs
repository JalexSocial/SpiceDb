namespace SpiceDb.Models;

/// <summary>
/// Represents an item in a bulk permission check request.
/// This mirrors the CheckBulkPermissionsRequestItem message in the protobuf.
/// </summary>
public class CheckBulkPermissionsRequestItem
{
	/// <summary>
	/// Gets or sets the permission to be checked.
	/// </summary>
	public Permission Permission { get; set; } = default!;

	/// <summary>
	/// Gets or sets the context information (e.g. caveat parameters) to be used during the check.
	/// </summary>
	public Dictionary<string, object> Context { get; set; } = new();
}