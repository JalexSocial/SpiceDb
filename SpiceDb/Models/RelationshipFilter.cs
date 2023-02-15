using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;

public class RelationshipFilter
{
    public string Type { get; set; } = string.Empty;
    public string OptionalRelation { get; set; } = string.Empty;
    public string OptionalId { get; set; } = string.Empty;
}
