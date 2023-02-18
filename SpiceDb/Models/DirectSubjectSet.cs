using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;

public class DirectSubjectSet
{
	public List<ResourceReference> Subjects { get; set; } = new();
}
