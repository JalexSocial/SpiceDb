using Authzed.Api.V1;
using Grpc.Core;
using SpiceDb.Enum;

namespace SpiceDb.Api;

internal class SpiceDbExperimental
{
    private readonly ExperimentalService.ExperimentalServiceClient _exp;
    private readonly CallOptions _callOptions;

    public SpiceDbExperimental(ChannelBase channel, CallOptions callOptions)
    {
        _exp = new ExperimentalService.ExperimentalServiceClient(channel);
        _callOptions = callOptions;
    }
}