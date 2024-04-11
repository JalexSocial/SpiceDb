using Grpc.Core;
using Grpc.Net.Client;
using System.Net;

namespace SpiceDb.Api;

internal class SpiceDbCore
{
    public readonly SpiceDbPermissions Permissions;
    public readonly SpiceDbSchema Schema;
    public readonly SpiceDbWatch Watch;
    public readonly SpiceDbExperimental Experimental;

    /// <summary>
    /// Example:
    /// serverAddress   "http://localhost:50051"
    /// preSharedKey    "spicedb_token"
    /// </summary>
    public SpiceDbCore(ChannelBase channel)
    {
        Permissions = new SpiceDbPermissions(channel);
        Watch = new SpiceDbWatch(channel);
        Schema = new SpiceDbSchema(channel);
        Experimental = new SpiceDbExperimental(channel);
    }
}


