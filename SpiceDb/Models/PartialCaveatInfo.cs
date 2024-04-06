namespace SpiceDb.Models;

public class PartialCaveatInfo
{
    public List<string> MissingRequiredContext { get; set; } = new List<string>();
}