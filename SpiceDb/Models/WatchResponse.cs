namespace SpiceDb.Models;

/// <summary>
/// Represents a response received from a watch operation.
/// Contains a token up to which changes have been observed and a list of relationship updates.
/// Mirrors the WatchResponse message in the protobuf.
/// </summary>
public class WatchResponse
{
	/// <summary>
	/// Gets or sets the token representing the state through which changes have been processed.
	/// </summary>
	public ZedToken? ChangesThrough { get; set; }

	/// <summary>
	/// Gets or sets the list of relationship update events.
	/// </summary>
	public List<RelationshipUpdate> Updates { get; set; } = new();
}
