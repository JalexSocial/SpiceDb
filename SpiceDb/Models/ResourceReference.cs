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

    /// <summary>
    /// Resource or subject reference
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <param name="optionalSubjectRelation">Note: Only subjects can have a relation</param>
    public ResourceReference(string type, string id, string optionalSubjectRelation = "")
    {
        this.Type = type;
        this.Id = id;
        this.Relation = optionalSubjectRelation;

        ProcessId();
    }

    public string Type { get; set; }
    public string Id { get; set; }
    public string Relation { get; set; } = string.Empty;

    private void ProcessId()
    {
        if (Id.Contains("#"))
        {
            var parts = Id.Split("#");
            Id = parts[0];
            Relation = parts[1];
        }
    }

    /// <summary>
    /// Sets up a resource reference that can reference a specific relation/permission inside an object
    /// For example: organization:easd may be an original reference, but to reference members of easd
    /// the reference should be: organization:easd#members 
    /// </summary>
    /// <param name="relation"></param>
    /// <returns></returns>
    public ResourceReference WithSubjectRelation(string relation) => new ResourceReference(this.Type, this.Id, relation);

    public ResourceReference EnsurePrefix(string prefix)
    {
        var type = this.Type;
        type = string.IsNullOrEmpty(type) ? type : type.StartsWith(prefix + "/") ? type : $"{prefix}/{type}";

        if (type == this.Type)
            return this;

        return new ResourceReference(type, this.Id, this.Relation);
    }

    public ResourceReference ExcludePrefix(string prefix)
    {
        var type = this.Type;

        if (!prefix.EndsWith("/")) prefix += "/";

        type = type.StartsWith(prefix) ? type.Substring(prefix.Length) : type;

        if (type == this.Type)
            return this;

        return new ResourceReference(type, this.Id, this.Relation);
    }

    public override string ToString() => $"{this.Type}:{this.Id}" + (String.IsNullOrEmpty(Relation) ? "" : $"#{Relation}");
}
