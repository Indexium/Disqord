using System.Net;
using Disqord.Http;
using Disqord.Http.Default;

namespace Disqord.Tests.Unit.Rest;

public class DefaultHttpClientTests
{
    [TestCase(1, 1, HttpVersionPolicy.RequestVersionOrLower)]
    [TestCase(2, 0, HttpVersionPolicy.RequestVersionOrLower)]
    [TestCase(2, 0, HttpVersionPolicy.RequestVersionExact)]
    public async Task SendAsync_UsesConfiguredHttpVersion(int major, int minor, HttpVersionPolicy policy)
    {
        var version = new Version(major, minor);
        using var handler = new InspectingHandler();
        using var http = new HttpClient(handler)
        {
            DefaultRequestVersion = version,
            DefaultVersionPolicy = policy
        };
        using var client = new DefaultHttpClient(http);
        using var request = new DefaultHttpRequest(HttpRequestMethod.Post, new Uri("https://example.invalid/upload"), null);
        using var response = await client.SendAsync(request, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Version, Is.EqualTo(version));
            Assert.That(handler.Policy, Is.EqualTo(policy));
        });
    }

    private sealed class InspectingHandler : HttpMessageHandler
    {
        public Version? Version { get; private set; }
        public HttpVersionPolicy Policy { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Version = request.Version;
            Policy = request.VersionPolicy;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
