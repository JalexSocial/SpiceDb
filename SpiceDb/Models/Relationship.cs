namespace SpiceDb.Models;

public class Relationship
{
    public Relationship(ResourceReference resource, string relation, ResourceReference subject)
    {
        Resource = resource;
        Relation = relation;
        Subject = subject;
    }

    public Relationship(string resource, string relation, string subject)
    {
        Resource = new ResourceReference(resource);
        Relation = relation;
        Subject = new ResourceReference(subject);
    }

    /// <summary>
    /// Creates a permission based on the format resource#relation@subject
    /// Example: user:1234#view@user:1 (can user:1 view user:1234?)
    /// </summary>
    /// <param name="permission"></param>
    /// <exception cref="ArgumentException"></exception>
    public Relationship(string relation)
    {
        var parts = relation.Split(new char[] {'#', '@'}, 3);

        if (parts.Length != 3)
            throw new ArgumentException($"Bad {this.GetType().Name.ToLower()} string provided");

        Resource = new ResourceReference(parts[0]);
        Relation = parts[1];
        Subject = new ResourceReference(parts[2]);
    }

    public ResourceReference Resource { get; set; }
    public string Relation { get; set; }
    public ResourceReference Subject { get; set; }

    public override string ToString()
    {
        return $"{Resource.ToString()}#{this.Relation}@{Subject.ToString()}";
    }
}
