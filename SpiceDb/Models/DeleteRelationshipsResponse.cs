using SpiceDb.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;

public class DeleteRelationshipsResponse
{
    public ZedToken? DeletedAt { get; set; }
    public DeletionProgress DeletionProgress { get; set; } = DeletionProgress.Unspecified;
}
