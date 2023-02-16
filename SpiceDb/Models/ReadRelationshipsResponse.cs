using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authzed.Api.V1;

namespace SpiceDb.Models;

public class ReadRelationshipsResponse
{
    public ZedToken? Token { get; set; }
    public Relationship Relationship { get; set; } = null!;
}
