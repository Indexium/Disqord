using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Disqord.Http;

public class MultipartFormDataHttpRequestContent : HttpRequestContent
{
    public string Boundary { get; }

    public List<(HttpRequestContent Content, string Name, string? FileName)> FormData { get; }

    public MultipartFormDataHttpRequestContent(string boundary)
    {
        Boundary = boundary;
        FormData = new List<(HttpRequestContent, string, string?)>();
    }

    /// <inheritdoc/>
    public override async ValueTask RewindAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < FormData.Count; i++)
        {
            var data = FormData[i];
            await data.Content.RewindAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        for (var i = 0; i < FormData.Count; i++)
        {
            var data = FormData[i];
            data.Content.Dispose();
        }
    }
}
