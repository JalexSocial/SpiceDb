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

internal class SpiceDbSchema
{
    private SchemaService.SchemaServiceClient? _schema;
    private readonly CallOptions _callOptions;

    public SpiceDbSchema(ChannelBase channel, CallOptions callOptions)
    {
        _schema = new SchemaService.SchemaServiceClient(channel);
        _callOptions = callOptions;
    }

    public async Task<string> ReadSchemaAsync()
    {
        ReadSchemaRequest req = new ReadSchemaRequest();
        ReadSchemaResponse resp = await _schema!.ReadSchemaAsync(req, _callOptions);
        return resp.SchemaText;
    }

    public async Task<WriteSchemaResponse> WriteSchemaAsync(string schema)
    {
        var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";

        schema = Regex.Replace(schema, re, "$1");

        WriteSchemaRequest req = new WriteSchemaRequest
        {
            Schema = schema
        };
        return await _schema!.WriteSchemaAsync(req, _callOptions);
    }
}