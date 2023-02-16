using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authzed.Api.V1;

namespace SpiceDb.Models;

public class WatchResponse
{
    public ZedToken? ChangesThrough { get; set; }
    public List<SpiceDb.Models.RelationshipUpdate> Updates { get; set; } = new();
}
