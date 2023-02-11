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
    }

    public ResourceReference(string type, string id)
    {
        this.Type = type;
        this.Id = id;
    }

    public string Type { get; set; }
    public string Id { get; set; }

    public string AsFullKey() => $"{Type}:{Id}";
}
