namespace SpiceDb.Models;

public class BulkCheckPermissionResponse
{
	public ZedToken? CheckedAt { get; set; }
	public List<BulkCheckPermissionPair> Pairs { get; set; } = new List<BulkCheckPermissionPair>();
}