using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class Precondition
{
    public PreconditionOperation Operation { get; set; }
    public RelationshipFilter Filter { get; set; } = new();
    public RelationshipFilter? OptionalSubjectFilter { get; set; } 
}
