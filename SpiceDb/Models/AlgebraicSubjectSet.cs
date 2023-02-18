using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceDb.Enum;

namespace SpiceDb.Models;

public class AlgebraicSubjectSet
{
	public AlgebraicSubjectSetOperation Operation { get; set; } = AlgebraicSubjectSetOperation.Unspecified;
	public List<PermissionRelationshipTree> Children { get; set; } = new();
}
