using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Enum;
public enum RelationshipUpdateOperation
{
    Create = 1,
    Upsert = 2,
    Delete = 3
}
