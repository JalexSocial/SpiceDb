using Grpc.Core;

namespace SpiceDb.Api;

internal class SpiceDbCore
{
    public readonly SpiceDbPermissions Permissions;
    public readonly SpiceDbSchema Schema;
    public readonly SpiceDbWatch Watch;
    public readonly SpiceDbExperimental Experimental;

    /// <summary>
    /// Core container for various library sections
    /// </summary>
    public SpiceDbCore(ChannelBase channel)
    {
        Permissions = new SpiceDbPermissions(channel);
        Watch = new SpiceDbWatch(channel);
        Schema = new SpiceDbSchema(channel);
        Experimental = new SpiceDbExperimental(channel);
    }
}


