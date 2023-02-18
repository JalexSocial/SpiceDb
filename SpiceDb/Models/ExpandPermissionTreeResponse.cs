using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;
public class ExpandPermissionTreeResponse
{
	public ZedToken ExpandedAt { get; set; } = null!;
	public PermissionRelationshipTree? TreeRoot { get; set; } = null;
}
