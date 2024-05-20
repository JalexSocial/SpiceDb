namespace SpiceDb.Models;

/// <summary>
/// RelationshipFilter is a collection of filters which when applied to a relationship will return
/// relationships that have exactly matching fields.
/// All fields are optional and if left unspecified will not filter relationships, but at least one
/// field must be specified.
/// </summary>
public class RelationshipFilter
{
    /// <summary>
    /// Optional resource type of teh relationship
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Optional relation of the relationship
    /// </summary>
    public string OptionalRelation { get; set; } = string.Empty;
    /// <summary>
    /// Optional Id of the relationship
    /// </summary>
    public string OptionalId { get; set; } = string.Empty;
}
