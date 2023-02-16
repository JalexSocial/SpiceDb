using SpiceDb.Enum;

namespace SpiceDb.Models;

public class RelationshipUpdate
{
    public Relationship Relationship { get; set; } = null!;
    public RelationshipUpdateOperation Operation { get; set; }
}
