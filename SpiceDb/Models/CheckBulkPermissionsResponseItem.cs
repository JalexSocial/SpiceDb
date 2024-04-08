using SpiceDb.Enum;

namespace SpiceDb.Models;

public class CheckBulkPermissionsResponseItem
{
    public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public PartialCaveatInfo? PartialCaveatInfo { get; set; }
}