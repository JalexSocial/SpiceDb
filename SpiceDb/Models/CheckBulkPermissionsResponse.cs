namespace SpiceDb.Models;

public class CheckBulkPermissionsResponse
{
    public ZedToken? CheckedAt { get; set; }
    public List<CheckBulkPermissions> Pairs { get; set; } = new List<CheckBulkPermissions>();
}