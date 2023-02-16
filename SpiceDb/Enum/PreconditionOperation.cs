using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Enum;

public enum PreconditionOperation
{
    Unspecified = 0,
    MustNotMatch = 1,
    MustMatch = 2
}
