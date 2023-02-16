using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authzed.Api.V1;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class LookupResourcesResponse
{
    public ZedToken? LookedUpAt { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public SpiceDb.Enum.Permissionship Permissionship { get; set; } = Permissionship.Unspecified;
    public List<string> MissingRequiredContext { get; set; } = new();
}
