namespace SpiceDb.Models;
public class ExpandPermissionTreeResponse
{
    public ZedToken ExpandedAt { get; set; } = null!;
    public PermissionRelationshipTree? TreeRoot { get; set; } = null;
}
