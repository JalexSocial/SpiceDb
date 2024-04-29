using SpiceDb.Abstractions;

namespace SpiceDb.Models;

public class RelationshipFilter : IRelationshipFilter
{
    public string Type { get; set; } = string.Empty;
    public string OptionalRelation { get; set; } = string.Empty;
    public string OptionalId { get; set; } = string.Empty;
}
