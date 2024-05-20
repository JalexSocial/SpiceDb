using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Enum;

public enum DeletionProgress
{
    Unspecified = 0,

    /// <summary>
    /// Indicates that all remaining relationships matching the filter were deleted. Will be returned
    /// even if no relationships were deleted.
    /// </summary>
    Complete = 1,

    /// <summary>
    /// Indicates that a subset of the relationships matching the filter
    /// were deleted. Only returned if optional_allow_partial_deletions was true, an optional_limit was
    /// specified, and there existed more relationships matching the filter than optional_limit would allow.
    /// Once all remaining relationships have been deleted, DeletionProgress.Complete will be returned.
    /// </summary>
    Partial = 2
}
