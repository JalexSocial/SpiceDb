namespace SpiceDb.Models;

/// <summary>
/// Represents the response from a bulk permission check request.
/// Contains the token at which the checks were performed and a list of individual check results.
/// </summary>
public class CheckBulkPermissionsResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the permission system when the checks were performed.
	/// </summary>
	public ZedToken? CheckedAt { get; set; }

	/// <summary>
	/// Gets or sets the list of individual permission check result pairs.
	/// </summary>
	public List<CheckBulkPermissions> Pairs { get; set; } = new List<CheckBulkPermissions>();
}