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

    public async Task<SpiceDb.Models.BulkCheckPermissionResponse?> BulkCheckPermissionAsync(IEnumerable<BulkCheckPermissionRequestItem> items, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
    {
        var req = new BulkCheckPermissionRequest()
        {
            Consistency = new Consistency { MinimizeLatency = true, AtExactSnapshot = zedToken },
        };

        req.Items.AddRange(items);

        if (cacheFreshness == CacheFreshness.AtLeastAsFreshAs)
        {
            req.Consistency.AtLeastAsFresh = zedToken;
        }
        else if (cacheFreshness == CacheFreshness.MustRefresh || zedToken == null)
        {
            req.Consistency.FullyConsistent = true;
        }

        var call = await _exp.BulkCheckPermissionAsync(req, options: _callOptions);

        if (call == null)
            return null;

        SpiceDb.Models.BulkCheckPermissionResponse response = new SpiceDb.Models.BulkCheckPermissionResponse
        {
            CheckedAt = call.CheckedAt.ToSpiceDbToken(),
            Pairs = call.Pairs.Select(x => new Models.BulkCheckPermission
            {
                Error = x.Error is null
                    ? null
                    : new Models.Status
                    {
                        Code = x.Error.Code,
                        Message = x.Error.Message,
                        Details = x.Error.Details.Select(any => (object)any).ToList()
                    },
                PartialCaveatInfo = x.Item.PartialCaveatInfo is null
                    ? null
                    : new SpiceDb.Models.PartialCaveatInfo
                    { MissingRequiredContext = x.Item.PartialCaveatInfo.MissingRequiredContext.ToList() },
                Permissionship = (Permissionship)x.Item.Permissionship,
                Permission = new Models.Permission(x.Request.Permission),
                Context = x.Request.Context.FromStruct()
            }).ToList()
        };

        return response;
    }
}