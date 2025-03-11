using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents a node within the permission relationship tree.
/// A node can be either an intermediate node representing an algebraic operation or a leaf node containing direct subjects.
/// </summary>
public class PermissionRelationshipTree
{
	/// <summary>
	/// Gets or sets the expanded object reference at this node.
	/// </summary>
	public ResourceReference? ExpandedObject { get; set; }

	/// <summary>
	/// Gets or sets the relation associated with the expanded object.
	/// </summary>
	public string ExpandedRelation { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the type of the tree node.
	/// Maps to the TreeType (None, Intermediate, or Leaf) as defined in the protobuf.
	/// </summary>
	public TreeType TreeType { get; set; } = TreeType.None;

	/// <summary>
	/// Gets or sets the intermediate algebraic subject set if this node represents a set operation.
	/// </summary>
	public AlgebraicSubjectSet? Intermediate { get; set; }

	/// <summary>
	/// Gets or sets the direct subject set if this node is a leaf.
	/// </summary>
	public DirectSubjectSet? Leaf { get; set; }
}
