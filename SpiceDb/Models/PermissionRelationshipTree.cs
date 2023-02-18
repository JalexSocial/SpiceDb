using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class PermissionRelationshipTree
{
    public ResourceReference? ExpandedObject { get; set; }
    public string ExpandedRelation { get; set; } = string.Empty;
    public TreeType TreeType { get; set; } = TreeType.None;
    public AlgebraicSubjectSet? Intermediate { get; set; }
    public DirectSubjectSet? Leaf { get; set; }
}
