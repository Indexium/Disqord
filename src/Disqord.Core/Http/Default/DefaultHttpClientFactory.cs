using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace Disqord.Http.Default;

public class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
{
    private readonly SocketsHttpHandler _handler;
    private readonly Version _defaultRequestVersion;
    private readonly HttpVersionPolicy _defaultVersionPolicy;

    public DefaultHttpClientFactory(
        IOptions<DefaultHttpClientFactoryConfiguration> options)
    {
        _defaultRequestVersion = options.Value.DefaultRequestVersion;
        _defaultVersionPolicy = options.Value.DefaultVersionPolicy;
        _handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };

        options.Value.ClientConfiguration?.Invoke(_handler);
    }

    public IHttpClient CreateClient()
    {
        var client = new HttpClient(_handler, false)
        {
            Timeout = TimeSpan.FromMinutes(2),
            DefaultRequestVersion = _defaultRequestVersion,
            DefaultVersionPolicy = _defaultVersionPolicy
        };

        return new DefaultHttpClient(client);
    }

    public void Dispose()
    {
        _handler.Dispose();
    }
}
