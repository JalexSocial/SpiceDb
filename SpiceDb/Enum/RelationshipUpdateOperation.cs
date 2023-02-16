namespace SpiceDb.Enum;

public enum RelationshipUpdateOperation
{
    /// <summary>
    /// Create the relationship only if it doesn't exist, and error otherwise.
    /// </summary>
    Create = 1,

    /// <summary>
    /// Upsert the relationship, and will not error if it already exists.
    /// </summary>
    Upsert = 2,

    /// <summary>
    /// Delete the relationship. If the relationship does not exist, this operation will no-op.
    /// </summary>
    Delete = 3
}
