using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Http.Default;
using Disqord.Rest.Api;
using Disqord.Serialization.Json.Default;

namespace Disqord.Tests.Unit.Rest;

public class HttpRequestContentRewindTests
{
    [Test]
    public async Task RewindAsync_SeekableStreamResentAfterRetry_SecondSendContainsTheFullContentAgain()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var content = new MultipartRestRequestContent();
        content.Add(new MemoryStream(bytes), "files[0]", "file.txt");

        var httpRequestContent = content.CreateHttpContent(new DefaultJsonSerializer());

        // Act
        await httpRequestContent.RewindAsync(CancellationToken.None);
        var firstBody = await ((MultipartFormDataContent) DefaultHttpClient.GetHttpContent(httpRequestContent)).ReadAsStringAsync();

        await httpRequestContent.RewindAsync(CancellationToken.None);
        var secondBody = await ((MultipartFormDataContent) DefaultHttpClient.GetHttpContent(httpRequestContent)).ReadAsStringAsync();

        // Assert
        Assert.That(firstBody, Does.Contain("hello world"));
        Assert.That(secondBody, Does.Contain("hello world"));
    }

    [Test]
    public async Task RewindAsync_NonSeekableStreamResentAfterRetry_SecondSendContainsTheFullContentAgain()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var content = new MultipartRestRequestContent();
        content.Add(new ForwardOnlyStream(bytes), "files[0]", "file.txt");

        var httpRequestContent = content.CreateHttpContent(new DefaultJsonSerializer());

        // Act
        await httpRequestContent.RewindAsync(CancellationToken.None);
        var firstBody = await ((MultipartFormDataContent) DefaultHttpClient.GetHttpContent(httpRequestContent)).ReadAsStringAsync();

        await httpRequestContent.RewindAsync(CancellationToken.None);
        var secondBody = await ((MultipartFormDataContent) DefaultHttpClient.GetHttpContent(httpRequestContent)).ReadAsStringAsync();

        // Assert
        Assert.That(firstBody, Does.Contain("hello world"));
        Assert.That(secondBody, Does.Contain("hello world"));
    }

    [Test]
    public void Constructor_StreamPositionAtLength_ThrowsInvalidDataException()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3]);
        stream.Position = stream.Length;

        // Act & Assert
        Assert.That(() => new StreamHttpRequestContent(stream), Throws.InstanceOf<InvalidDataException>());
    }

    private sealed class ForwardOnlyStream : MemoryStream
    {
        public override bool CanSeek => false;

        public ForwardOnlyStream(byte[] buffer)
            : base(buffer)
        { }
    }
}
