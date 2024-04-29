namespace SpiceDb.Abstractions;

public interface ICaveat
{
    string Name { get; set; }
    Dictionary<string, object> Context { get; set; }
}