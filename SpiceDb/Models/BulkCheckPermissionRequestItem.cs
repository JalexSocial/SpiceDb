namespace SpiceDb.Models;

public class BulkCheckPermissionRequestItem
{
	public Permission? Permission { get; set; }
	public Dictionary<string, object> Context { get; set; } = new();
}