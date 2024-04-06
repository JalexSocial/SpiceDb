namespace SpiceDb.Models;

public class BulkCheckPermissionRequestItem
{
	public Permission Permission { get; set; } = default!;
	public Dictionary<string, object> Context { get; set; } = new();
}