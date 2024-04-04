namespace SpiceDb.Models;

public class BulkCheckPermissionPair
{
	public BulkCheckPermissionRequestItem? Request { get; set; }
	public BulkCheckPermissionResponseItem? Item { get; set; }
	public Status? Error { get; set; }
}