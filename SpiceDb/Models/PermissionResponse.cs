using Authzed.Api.V1;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class PermissionResponse
{
    public Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public ZedToken? ZedToken { get; set; }
    public bool HasPermission => Permissionship is Permissionship.HasPermission;
}
