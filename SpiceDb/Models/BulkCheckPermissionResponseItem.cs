using SpiceDb.Enum;

namespace SpiceDb.Models;

public class BulkCheckPermissionResponseItem
{
    public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public PartialCaveatInfo? PartialCaveatInfo { get; set; }
}