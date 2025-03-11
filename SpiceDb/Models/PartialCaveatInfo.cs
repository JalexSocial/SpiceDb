namespace SpiceDb.Models;

/// <summary>
/// Contains information about a caveat that was only partially evaluated.
/// Specifically, lists the context keys that were missing during evaluation.
/// </summary>
public class PartialCaveatInfo
{
	/// <summary>
	/// Gets or sets the list of missing context keys.
	/// </summary>
	public List<string> MissingRequiredContext { get; set; } = new List<string>();
}