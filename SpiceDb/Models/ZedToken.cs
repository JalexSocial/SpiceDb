using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Models;

public class ZedToken
{
	public ZedToken(string? token)
	{
		Token = token ?? string.Empty;
	}

    public string Token { get; set; }
}
