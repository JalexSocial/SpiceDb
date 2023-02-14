using Authzed.Api.V1;

namespace SpiceDb.Models;

public class PermissionResponse
{
    public bool HasPermission { get; set; }
    public ZedToken? ZedToken { get; set; }
}
