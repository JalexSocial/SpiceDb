namespace SpiceDb.Models;

public class CheckBulkPermissionsRequestItem
{
    public Permission Permission { get; set; } = default!;
    public Dictionary<string, object> Context { get; set; } = new();
}