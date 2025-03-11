using SpiceDb.Enum;

namespace SpiceDb.Models;

/// <summary>
/// Represents the resolved subject details returned by a lookup subjects operation.
/// </summary>
public class ResolvedSubject
{
	/// <summary>
	/// Gets or sets the subject identifier.
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the permissionship result for the subject.
	/// </summary>
	public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;

	/// <summary>
	/// Gets or sets any missing context keys if the subject was partially evaluated.
	/// </summary>
	public List<string> MissingRequiredContext { get; set; } = new();
}

/// <summary>
/// Represents a single response from a lookup subjects operation.
/// Contains the lookup token, the resolved subject, and any excluded subjects.
/// </summary>
public class LookupSubjectsResponse
{
	/// <summary>
	/// Gets or sets the token representing the state of the system when the subject lookup was performed.
	/// </summary>
	public ZedToken? LookedUpAt { get; set; }

	/// <summary>
	/// Gets or sets the resolved subject.
	/// </summary>
	public ResolvedSubject Subject { get; set; } = new();

	/// <summary>
	/// Gets or sets the list of subjects excluded (for example, when a wildcard was matched).
	/// </summary>
	public List<ResolvedSubject> ExcludedSubjects { get; set; } = new();
}