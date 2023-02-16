namespace SpiceDb.Models;

public class RelationshipFilter
{
    public string Type { get; set; } = string.Empty;
    public string OptionalRelation { get; set; } = string.Empty;
    public string OptionalId { get; set; } = string.Empty;
}
