namespace SpiceDb.Abstractions;

public interface IRelationshipFilter
{
    string Type { get; set; }
    string OptionalRelation { get; set; }
    string OptionalId { get; set; }
}