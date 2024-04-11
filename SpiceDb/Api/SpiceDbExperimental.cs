using Authzed.Api.V1;
using Grpc.Core;
using SpiceDb.Enum;

namespace SpiceDb.Api;

internal class SpiceDbExperimental
{
    private readonly ExperimentalService.ExperimentalServiceClient _exp;

    public SpiceDbExperimental(ChannelBase channel)
    {
        _exp = new ExperimentalService.ExperimentalServiceClient(channel);
    }
}