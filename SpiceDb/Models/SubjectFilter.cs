namespace SpiceDb.Models;

public class SubjectFilter
{
    public required string Type { get; set; }
    public string? OptionalRelation { get; set; }
    public string OptionalId { get; set; } = string.Empty;
}