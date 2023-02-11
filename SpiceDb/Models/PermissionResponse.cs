using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authzed.Api.V1;

namespace SpiceDb.Models;

public class PermissionResponse
{
    public bool HasPermission { get; set; }
    public ZedToken? ZedToken { get; set; }
}
