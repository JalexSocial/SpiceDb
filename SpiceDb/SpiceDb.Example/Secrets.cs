using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceDb.Example;
public class Secrets
{
    public string ServerAddress { get; set; } = "https://grpc.authzed.com";
    public string Token { get; set; } = "token_abcdefghijklmnop";
}
