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
        _preSharedKey = preSharedKey;

        if (serverAddress.StartsWith("http:"))
        {
            _headers = new()
            {
                { "Authorization", $"Bearer {_preSharedKey}" }
            };

            callOptions = new CallOptions(_headers);
        }
        else if (serverAddress.StartsWith("https:"))
        {
            callOptions = new CallOptions();
        }
        else
        {
            throw new ArgumentException("Expecting http or https in the authzed endpoint.");
        }

        var channel = CreateAuthenticatedChannel(serverAddress);

        Permissions = new SpiceDbPermissions(channel, callOptions, _headers);
        Watch = new SpiceDbWatch(channel, _headers);
        Schema = new SpiceDbSchema(channel, callOptions);
        Experimental = new SpiceDbExperimental(channel, callOptions);
    }

    private ChannelBase CreateAuthenticatedChannel(string address)
    {
        var token = _preSharedKey;
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
	        channel = GrpcChannel.ForAddress(address);
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
}


