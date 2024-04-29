namespace SpiceDb.Abstractions;

public interface IRelationship
{
    /// <summary>
    /// Resource is the resource to which the subject is related, in some manner
    /// </summary>
    IResourceReference Resource { get; set; }

    /// <summary>
    /// Relation is how the resource and subject are related.
    /// </summary>
    string Relation { get; set; }

    /// <summary>
    /// Subject is the subject to which the resource is related, in some manner.
    /// </summary>
    IResourceReference Subject { get; set; }

    /// <summary>
    /// OptionalCaveat is a reference to a the caveat that must be enforced over the relationship
    /// </summary>
    ICaveat? OptionalCaveat { get; set; }
}