using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Enum;

public enum CacheFreshness
{
    AnyFreshness,
    AtLeastAsFreshAs,
    MustRefresh
}
