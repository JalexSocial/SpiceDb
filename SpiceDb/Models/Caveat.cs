namespace SpiceDb.Models;

/// <summary>
/// Represents a caveat condition that must be met for a relationship’s permission to be valid.
/// Corresponds to the contextualized caveat defined in the core.proto.
/// </summary>
public class Caveat
{
	/// <summary>
	/// Gets or sets the name of the caveat expression.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the context parameters required for evaluating the caveat.
	/// </summary>
	public Dictionary<string, object> Context { get; set; } = new();
}
