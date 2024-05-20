namespace SpiceDb.Models;

public class ReadRelationshipsResponse
{
    /// <summary>
    /// ZedToken at which the relationship was found.
    /// </summary>
    public ZedToken? Token { get; set; }
    /// <summary>
    /// Relationship is the found relationship.
    /// </summary>
    public Relationship Relationship { get; set; } = null!;
    /// <summary>
    /// Cursor that can be used to resume the ReadRelationships stream after this result.
    /// </summary>
    public Cursor? AfterResultCursor { get; set; }
}
