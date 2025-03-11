using Authzed.Api.V1;
using Grpc.Core;
using SpiceDb.Enum;
using SpiceDb.Models;
using System.Runtime.CompilerServices;
using RelationshipUpdate = Authzed.Api.V1.RelationshipUpdate;
using ZedToken = Authzed.Api.V1.ZedToken;

namespace SpiceDb.Api;

internal class SpiceDbWatch
{
    private readonly WatchService.WatchServiceClient? _watch;

    public SpiceDbWatch(ChannelBase channel)
    {
        _watch = new WatchService.WatchServiceClient(channel);
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

        var call = _watch!.Watch(request, null, deadline, cancellationToken);

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
                        },
                        optionalExpiresAt: x.Relationship.OptionalExpiresAt),
						
                }).ToList()
            };

        }
    }
}