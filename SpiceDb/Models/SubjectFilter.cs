namespace SpiceDb.Models;

/// <summary>
/// Specifies a filter on the subject of a relationship.
/// </summary>
public class SubjectFilter
{
    /// <summary>
    /// Required subject type of the relationship
    /// </summary>
    public required string Type { get; set; }
    /// <summary>
    /// Optional relation of the relationship
    /// </summary>
    public string? OptionalRelation { get; set; }
    /// <summary>
    /// Optional resource ID of the relationship
    /// </summary>
    public string OptionalId { get; set; } = string.Empty;
}