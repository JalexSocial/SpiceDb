using System.Net;
using System.Runtime.CompilerServices;
using Authzed.Api.V1;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using SpiceDb.Enum;
using SpiceDb.Models;
using System.Text.RegularExpressions;
using LookupResourcesResponse = Authzed.Api.V1.LookupResourcesResponse;
using Precondition = Authzed.Api.V1.Precondition;
using Relationship = Authzed.Api.V1.Relationship;
using RelationshipUpdate = Authzed.Api.V1.RelationshipUpdate;
using ZedToken = Authzed.Api.V1.ZedToken;
using System.Threading.Channels;

namespace SpiceDb.Api;

internal class SpiceDbWatch
{
    private readonly WatchService.WatchServiceClient? _watch;
    private readonly Metadata? _headers;

    public SpiceDbWatch(ChannelBase channel, Metadata? headers)
    {
        _watch = new WatchService.WatchServiceClient(channel);
        _headers = headers;
    }

    public async IAsyncEnumerable<SpiceDb.Models.WatchResponse> Watch(List<string>? optionalSubjectTypes = null,
        ZedToken? zedToken = null,
        DateTime? deadline = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new WatchRequest
        {
            OptionalStartCursor = zedToken
        };

        request.OptionalObjectTypes.AddRange(optionalSubjectTypes ?? new List<string>());

        var call = _watch!.Watch(request, _headers, deadline, cancellationToken);

        await foreach (var resp in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            if (resp is null) continue;

            yield return new SpiceDb.Models.WatchResponse
            {
                ChangesThrough = resp.ChangesThrough.ToSpiceDbToken(),
                Updates = resp.Updates.Select(x => new SpiceDb.Models.RelationshipUpdate
                {
                    Operation = x.Operation switch
                    {
                        RelationshipUpdate.Types.Operation.Create => RelationshipUpdateOperation.Create,
                        RelationshipUpdate.Types.Operation.Delete => RelationshipUpdateOperation.Delete,
                        RelationshipUpdate.Types.Operation.Touch => RelationshipUpdateOperation.Upsert,
                        _ => RelationshipUpdateOperation.Upsert
                    },
                    Relationship = new SpiceDb.Models.Relationship(
                        resource: new ResourceReference(x.Relationship.Resource.ObjectType, x.Relationship.Resource.ObjectId),
                        relation: x.Relationship.Relation,
                        subject: new ResourceReference(x.Relationship.Subject.Object.ObjectType, x.Relationship.Subject.Object.ObjectId, x.Relationship.Subject.OptionalRelation),
                        optionalCaveat: new Caveat
                        {
                            Name = x.Relationship.OptionalCaveat.CaveatName,
                            Context = x.Relationship.OptionalCaveat.Context.FromStruct()
                        })
                }).ToList()
            };

        }
    }
}