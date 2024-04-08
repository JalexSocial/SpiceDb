using SpiceDb.Enum;

namespace SpiceDb.Models;

public class CheckBulkPermissions
{
    public Permission Permission { get; set; } = default!;
    public Dictionary<string, object> Context { get; set; } = new();
    public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public bool HasPermission => Permissionship is Permissionship.HasPermission;
    public PartialCaveatInfo? PartialCaveatInfo { get; set; }
    public Status? Error { get; set; }
}