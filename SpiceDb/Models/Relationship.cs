namespace SpiceDb.Models;

public class Relationship
{
    public Relationship(ResourceReference resource, string relation, ResourceReference subject, Caveat? optionalCaveat = null)
    {
        Resource = resource;
        Relation = relation;
        Subject = subject;

        if (!string.IsNullOrEmpty(Resource.Relation))
        {
            throw new ArgumentException("Error: Resource cannot have a relation");
        }
    }

    public Relationship(string resource, string relation, string subject, Caveat? optionalCaveat = null)
    {
        Resource = new ResourceReference(resource);
        Relation = relation;
        Subject = new ResourceReference(subject);
        OptionalCaveat = optionalCaveat;

        if (!string.IsNullOrEmpty(Resource.Relation))
        {
            throw new ArgumentException("Error: Resource cannot have a relation");
        }
    }

    /// <summary>
    /// Creates a permission based on the format resource#relation@subject
    /// Example: user:1234#view@user:1 (can user:1 view user:1234?)
    /// </summary>
    /// <param name="relation"></param>
    /// <param name="optionalCaveat"></param>
    /// <exception cref="ArgumentException"></exception>
    public Relationship(string relation, Caveat? optionalCaveat = null)
    {
        var parts = relation.Split(new char[] { '#', '@' }, 3);

        if (parts.Length != 3)
            throw new ArgumentException($"Bad {this.GetType().Name.ToLower()} string provided");

        Resource = new ResourceReference(parts[0]);
        Relation = parts[1];
        Subject = new ResourceReference(parts[2]);
        OptionalCaveat = optionalCaveat;
    }

    /// <summary>
    /// Resource is the resource to which the subject is related, in some manner
    /// </summary>
    public ResourceReference Resource { get; set; }

    /// <summary>
    /// Relation is how the resource and subject are related.
    /// </summary>
    public string Relation { get; set; }

    /// <summary>
    /// Subject is the subject to which the resource is related, in some manner.
    /// </summary>
    public ResourceReference Subject { get; set; }

    /// <summary>
    /// OptionalCaveat is a reference to a the caveat that must be enforced over the relationship
    /// </summary>
    public Caveat? OptionalCaveat { get; set; }

    public override string ToString()
    {
        return $"{Resource.ToString()}#{this.Relation}@{Subject.ToString()}";
    }
}
