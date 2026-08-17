using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Qommon;

namespace Disqord.Http.Default;

public class StreamHttpRequestContent : HttpRequestContent
{
    public Stream Stream { get; private set; }

    public bool ShouldDispose { get; private set; }

    private readonly long _initialPosition;

    public StreamHttpRequestContent(Stream stream, bool shouldDispose = false)
    {
        Guard.IsNotNull(stream);

        if (stream.CanSeek && stream.Length != 0 && stream.Position == stream.Length)
        {
            Throw.InvalidDataException("The stream's position is the same as its length. Did you forget to rewind it?");
        }

        Stream = stream;
        ShouldDispose = shouldDispose;
        _initialPosition = stream.CanSeek ? stream.Position : 0;
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     If <see cref="Stream"/> is seekable, this seeks it back to the position it had when this
    ///     instance was created. Otherwise, since a non-seekable stream cannot be rewound, this reads
    ///     it fully into an internally-owned, seekable buffer the first time it is called, so subsequent
    ///     calls, i.e. retries, can rewind that buffer instead.
    /// </remarks>
    public override async ValueTask RewindAsync(CancellationToken cancellationToken)
    {
        if (Stream.CanSeek)
        {
            Stream.Seek(_initialPosition, SeekOrigin.Begin);
            return;
        }

        var buffer = new MemoryStream();
        await Stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;

        if (ShouldDispose)
            Stream.Dispose();

        Stream = buffer;
        ShouldDispose = true;
    }

    public override void Dispose()
    {
        if (ShouldDispose)
            Stream.Dispose();
    }
}