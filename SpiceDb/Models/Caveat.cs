namespace SpiceDb.Models;
public class Caveat
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}
