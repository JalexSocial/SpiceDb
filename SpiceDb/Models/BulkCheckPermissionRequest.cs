using Authzed.Api.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class BulkCheckPermissionRequest
{
	public CacheFreshness Consistency { get; set; }
	public List<BulkCheckPermissionRequestItem> Items { get; set; } = new List<BulkCheckPermissionRequestItem>();
}