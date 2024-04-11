using Authzed.Api.V1;
using Grpc.Core;
using System.Text.RegularExpressions;

namespace SpiceDb.Api;

internal class SpiceDbSchema
{
    private SchemaService.SchemaServiceClient? _schema;

    public SpiceDbSchema(ChannelBase channel)
    {
        _schema = new SchemaService.SchemaServiceClient(channel);
    }

    public async Task<string> ReadSchemaAsync()
    {
        ReadSchemaRequest req = new ReadSchemaRequest();
        ReadSchemaResponse resp = await _schema!.ReadSchemaAsync(req);
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
        return await _schema!.WriteSchemaAsync(req);
    }
}