namespace SpiceDb.Models;

public class Relationship
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Relationship"/> class.
	/// </summary>
	/// <param name="resource">The resource reference for the relationship.</param>
	/// <param name="relation">The name of the relation or permission.</param>
	/// <param name="subject">The subject reference.</param>
	/// <param name="optionalCaveat">An optional caveat to be enforced over the relationship.</param>
	/// <param name="optionalExpiresAt">An optional timestamp.</param>
	/// <exception cref="ArgumentException">Thrown if the resource reference already contains a relation.</exception>
    public Relationship(ResourceReference resource, string relation, ResourceReference subject, Caveat? optionalCaveat = null, Google.Protobuf.WellKnownTypes.Timestamp? optionalExpiresAt = null)
    {
        Resource = resource;
        Relation = relation;
        Subject = subject;
        OptionalCaveat = optionalCaveat;
        OptionalExpiresAt = optionalExpiresAt;

        if (!string.IsNullOrEmpty(Resource.Relation))
        {
            throw new ArgumentException("Error: Resource cannot have a relation");
        }

        OptionalExpiresAt = optionalExpiresAt;
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="Relationship"/> class using string parameters.
	/// </summary>
	/// <param name="resource">The resource identifier as a string.</param>
	/// <param name="relation">The relation or permission name.</param>
	/// <param name="subject">The subject identifier as a string.</param>
	/// <param name="optionalCaveat">An optional caveat for the relationship.</param>
	/// <param name="optionalExpiresAt">An optional timestamp.</param>
	/// <exception cref="ArgumentException">Thrown if the resource identifier contains a relation.</exception>
    public Relationship(string resource, string relation, string subject, Caveat? optionalCaveat = null, Google.Protobuf.WellKnownTypes.Timestamp? optionalExpiresAt = null)
    {
        Resource = new ResourceReference(resource);
        Relation = relation;
        Subject = new ResourceReference(subject);
        OptionalCaveat = optionalCaveat;
        OptionalExpiresAt = optionalExpiresAt;
         
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

    /// <summary>
    /// The time at which the relationship expires, if any.
    /// </summary>
    public Google.Protobuf.WellKnownTypes.Timestamp? OptionalExpiresAt { get; set; }

    public override string ToString()
    {
        return $"{Resource.ToString()}#{this.Relation}@{Subject.ToString()}";
    }
}
