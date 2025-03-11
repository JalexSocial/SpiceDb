using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents a precondition for write or delete operations on relationships.
/// Precondition operations (MustMatch or MustNotMatch) and the associated filter mirror the protobuf’s Precondition message.
/// </summary>
public class Precondition
{
	/// <summary>
	/// Gets or sets the operation that specifies how the existence of a relationship affects the request.
	/// </summary>
	public PreconditionOperation Operation { get; set; }

	/// <summary>
	/// Gets or sets the filter to apply on relationships for this precondition.
	/// </summary>
	public RelationshipFilter Filter { get; set; } = new();

	/// <summary>
	/// Gets or sets an optional subject filter to further narrow the precondition.
	/// </summary>
	public RelationshipFilter? OptionalSubjectFilter { get; set; }
}