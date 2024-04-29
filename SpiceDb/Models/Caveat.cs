using SpiceDb.Abstractions;

namespace SpiceDb.Models;

public class Caveat : ICaveat
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}
