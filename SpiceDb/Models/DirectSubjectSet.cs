namespace SpiceDb.Models;

/// <summary>
/// Represents a direct subject set (a leaf node) containing a list of subject references.
/// This is analogous to the leaf structure in the permission relationship tree from the protobuf.
/// </summary>
public class DirectSubjectSet
{
	/// <summary>
	/// Gets or sets the list of subject references.
	/// </summary>
	public List<ResourceReference> Subjects { get; set; } = new();
}
