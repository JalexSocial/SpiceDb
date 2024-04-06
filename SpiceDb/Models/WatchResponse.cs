namespace SpiceDb.Models;

public class WatchResponse
{
    public ZedToken? ChangesThrough { get; set; }
    public List<SpiceDb.Models.RelationshipUpdate> Updates { get; set; } = new();
}
