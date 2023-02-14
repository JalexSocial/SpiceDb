namespace SpiceDb.Models;
public class ResourceReference
{
    public ResourceReference(string type)
    {
        var parts = type.Split(':');

        if (parts.Length != 2)
            throw new ArgumentException("Invalid permission key - must have two parts only has '" + type + "'");

        Type = parts[0];
        Id = parts[1];

        ProcessId();
    }

    public ResourceReference(string type, string id, string relation = "")
    {
        this.Type = type;
        this.Id = id;
        this.Relation = relation;

        ProcessId();
    }

    public string Type { get; set; }
    public string Id { get; set; }
    public string Relation { get; set; } = string.Empty;

    private void ProcessId()
    {
	    if (Id.Contains("#"))
	    {
		    var parts = Id.Split(":");
		    Id = parts[0];
		    Relation = parts[1];
	    }
    }

    public string AsStringReference() => $"{Type}:{Id}";

    /// <summary>
    /// Sets up a resource reference that can reference a specific relation/permission inside an object
    /// For example: organization:easd may be an original reference, but to reference members of easd
    /// the reference should be: organization:easd#members 
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public ResourceReference WithSubjectRelation(string relation) => new ResourceReference(this.Type, this.Id, relation);
}
