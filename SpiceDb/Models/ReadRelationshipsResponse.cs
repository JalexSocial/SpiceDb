namespace SpiceDb.Models;

public class ReadRelationshipsResponse
{
    public ZedToken? Token { get; set; }
    public Relationship Relationship { get; set; } = null!;
}
