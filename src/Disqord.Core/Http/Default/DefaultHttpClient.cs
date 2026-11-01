using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Qommon;

namespace Disqord.Http.Default;

public sealed partial class DefaultHttpClient(HttpClient http) : IHttpClient
{
    public Uri? BaseUri
    {
        get => http.BaseAddress;
        set => http.BaseAddress = value;
    }

    public async Task<IHttpResponse> SendAsync(IHttpRequest request, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(request);

        using (var requestMessage = GetHttpMessage(request))
        {
            // HttpClient.SendAsync does not apply these defaults to an explicitly-created message.
            requestMessage.Version = http.DefaultRequestVersion;
            requestMessage.VersionPolicy = http.DefaultVersionPolicy;
            var completionOption = GetHttpCompletionOption(request.CompletionMode);
            var responseMessage = await http.SendAsync(requestMessage, completionOption, cancellationToken).ConfigureAwait(false);
            return new DefaultHttpResponse(responseMessage);
        }
    }

    public void SetDefaultHeader(string name, string value)
    {
        http.DefaultRequestHeaders.Set(name, value);
    }

    public void Dispose()
    {
        http.Dispose();
    }
}
