namespace SpiceDb.Models;

/// <summary>
/// Represents a single relationship read from the system.
/// Contains the token at which the relationship was found, the relationship details, and a pagination cursor.
/// </summary>
public class ReadRelationshipsResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the system when the relationship was read.
	/// </summary>
	public ZedToken? Token { get; set; }

	/// <summary>
	/// Gets or sets the relationship that was found.
	/// </summary>
	public Relationship Relationship { get; set; } = null!;

	/// <summary>
	/// Gets or sets the cursor that can be used to resume reading relationships.
	/// </summary>
	public Cursor? AfterResultCursor { get; set; }
}
