using Authzed.Api.V1;
using Grpc.Core;
using System.Text.RegularExpressions;

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