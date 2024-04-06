namespace SpiceDb.Models;

public class BulkCheckPermissionResponse
{
	public ZedToken? CheckedAt { get; set; }
	public List<BulkCheckPermission> Pairs { get; set; } = new List<BulkCheckPermission>();
}