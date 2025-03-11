namespace SpiceDb.Models;

/// <summary>
/// Represents the response of an expand permission tree operation.
/// Contains the token at which the expansion occurred and the root of the permission relationship tree.
/// </summary>
public class ExpandPermissionTreeResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the system when the tree was expanded.
	/// </summary>
	public ZedToken ExpandedAt { get; set; } = null!;

	/// <summary>
	/// Gets or sets the root of the permission relationship tree.
	/// This tree reveals how permissions are computed and may contain intermediate algebraic nodes or leaf subject sets.
	/// </summary>
	public PermissionRelationshipTree? TreeRoot { get; set; } = null;
}
