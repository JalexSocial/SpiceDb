using Grpc.Core;
using Grpc.Net.Client;
using System.Net;

namespace SpiceDb.Api;

internal class Core
{
    private readonly Metadata? _headers;
    private readonly string _preSharedKey;

    public readonly SpiceDbPermissions Permissions;
    public readonly SpiceDbSchema Schema;
    public readonly SpiceDbWatch Watch;
    public readonly SpiceDbExperimental Experimental;

    /// <summary>
    /// Example:
    /// serverAddress   "http://localhost:50051"
    /// preSharedKey    "spicedb_token"
    /// </summary>
    /// <param name="serverAddress"></param>
    /// <param name="preSharedKey"></param>
    public Core(string serverAddress, string preSharedKey)
    {
        CallOptions callOptions;
        var serverAddress1 = serverAddress;
        _preSharedKey = preSharedKey;

        if (serverAddress1.StartsWith("http:"))
        {
            _headers = new()
            {
                { "Authorization", $"Bearer {_preSharedKey}" }
            };

            callOptions = new CallOptions(_headers);
        }
        else if (serverAddress1.StartsWith("https:"))
        {
            callOptions = new CallOptions();
        }
        else
        {
            throw new ArgumentException("Expecting http or https in the authzed endpoint.");
        }

        var channel = CreateAuthenticatedChannelAsync(serverAddress1).GetAwaiter().GetResult();

        Permissions = new SpiceDbPermissions(channel, callOptions, _headers);
        Watch = new SpiceDbWatch(channel, _headers);
        Schema = new SpiceDbSchema(channel, callOptions);
        Experimental = new SpiceDbExperimental(channel, callOptions);
    }

    private async Task<ChannelBase> CreateAuthenticatedChannelAsync(string address)
    {
        var token = await GetTokenAsync();
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        //Support proxy by setting webproxy on httpClient
        HttpClient.DefaultProxy = new WebProxy();

        // SslCredentials is used here because this channel is using TLS.
        // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
        ChannelBase channel;
        if (address.StartsWith("http:"))
        {
            Uri baseUri = new Uri(address);
            channel = new Grpc.Core.Channel(baseUri.Host, baseUri.Port, ChannelCredentials.Insecure);
        }
        else
        {
            channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                //HttpHandler = handler,
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
        }
        return channel;
    }

    private Task<string> GetTokenAsync()
    {
        return Task.FromResult(_preSharedKey);
    }
}


