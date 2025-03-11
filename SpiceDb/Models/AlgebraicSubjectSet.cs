using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents an algebraic subject set computed from multiple permission trees using set operations.
/// The operation (union, intersection, exclusion) corresponds to the protobuf’s algebraic subject set.
/// </summary>
public class AlgebraicSubjectSet
{
	/// <summary>
	/// Gets or sets the set operation to combine the child trees.
	/// Maps to <c>AlgebraicSubjectSet.Operation</c> in the protobuf.
	/// </summary>
	public AlgebraicSubjectSetOperation Operation { get; set; } = AlgebraicSubjectSetOperation.Unspecified;

	/// <summary>
	/// Gets or sets the list of child permission relationship trees.
	/// </summary>
	public List<PermissionRelationshipTree> Children { get; set; } = new();
}
