using System.Net;
using System.Runtime.CompilerServices;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using SpiceDb.Enum;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Authzed.Api.V1;
using BulkCheckPermissionResponse = SpiceDb.Models.BulkCheckPermissionResponse;
using BulkCheckPermissionResponseItem = SpiceDb.Models.BulkCheckPermissionResponseItem;
using PartialCaveatInfo = SpiceDb.Models.PartialCaveatInfo;

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

    public SpiceDb.Models.BulkCheckPermissionResponse? BulkCheckPermission(IEnumerable<BulkCheckPermissionRequestItem> items, ZedToken? zedToken = null, CacheFreshness cacheFreshness = CacheFreshness.AnyFreshness)
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

        //Server streaming call, reads messages streamed from the service
        var call = _exp.BulkCheckPermission(req, options: _callOptions);

        if (call == null)
            return null;

        SpiceDb.Models.BulkCheckPermissionResponse response = new BulkCheckPermissionResponse
        {
            CheckedAt = call.CheckedAt.ToSpiceDbToken(),
            Pairs = call.Pairs.Select(x => new Models.BulkCheckPermissionPair
            {
                Error = x.Error is null ? null : new Models.Status { Code = x.Error.Code, Message = x.Error.Message, Details = x.Error.Details.Select(any => (object)any).ToList() },
                Item = x.Item is null ? null : new BulkCheckPermissionResponseItem
                {
                    PartialCaveatInfo = x.Item.PartialCaveatInfo is null ? null : new PartialCaveatInfo { MissingRequiredContext = x.Item.PartialCaveatInfo.MissingRequiredContext.ToList() },
                    Permissionship = (Permissionship)x.Item.Permissionship
                },
                Request = x.Request is null ? null : new Models.BulkCheckPermissionRequestItem
                {
                    Permission = new Models.Permission (x.Request.Permission),
                    Context = x.Request.Context.FromStruct()
                }
            }).ToList()
        };

        return response;
    }
}