using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class RelationshipUpdate
{
    public Relationship Relationship { get; set; } = null!;
    public RelationshipUpdateOperation Operation { get; set; }
}
