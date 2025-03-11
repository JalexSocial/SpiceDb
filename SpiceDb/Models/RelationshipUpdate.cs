using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents an update operation for a relationship.
/// The operation can create, update (upsert), or delete a relationship.
/// This class corresponds to the RelationshipUpdate message in the protobuf.
/// </summary>
public class RelationshipUpdate
{
	/// <summary>
	/// Gets or sets the relationship to be updated.
	/// </summary>
	public Relationship Relationship { get; set; } = null!;

	/// <summary>
	/// Gets or sets the operation to perform on the relationship.
	/// Maps to the RelationshipUpdateOperation enum in the protobuf.
	/// </summary>
	public RelationshipUpdateOperation Operation { get; set; }
}