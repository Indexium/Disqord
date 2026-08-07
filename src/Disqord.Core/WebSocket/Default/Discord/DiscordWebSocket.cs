using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.Logging;
using Qommon.Threading;
#if NET6_0_OR_GREATER
using System.IO.Compression;
#endif

namespace Disqord.WebSocket.Default.Discord
{
    internal sealed class DiscordWebSocket : IAsyncDisposable
    {
        public const int ReceiveBufferSize = 8192;

        public ILogger Logger { get; }

        private readonly IWebSocketClientFactory _webSocketClientFactory;
        private readonly bool _supportsZLib;

        private Connection? _connection;

        private readonly SemaphoreSlim _sendSemaphore;

        private readonly SemaphoreSlim _receiveSemaphore;
        private readonly byte[] _receiveBuffer;
        private readonly MemoryStream _receiveStream;
        private Stream? _receiveZLibStream;

        // Used to ensure the ZLib suffix was read after deserialization.
        private bool _wasLastPayloadZLib;

        private bool _isDisposed;

        public DiscordWebSocket(
            ILogger logger,
            IWebSocketClientFactory webSocketClientFactory,
            bool supportsZLib = true)
        {
            Logger = logger;
            _webSocketClientFactory = webSocketClientFactory;
            _supportsZLib = supportsZLib;

            _sendSemaphore = new SemaphoreSlim(1, 1);

            _receiveSemaphore = new SemaphoreSlim(1, 1);
            _receiveBuffer = new byte[ReceiveBufferSize];
            _receiveStream = new MemoryStream(ReceiveBufferSize * 2);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(null, "The Discord web socket client has been disposed.");
        }

        private Connection GetConnection()
        {
            var connection = _connection;
            if (connection == null)
            {
                throw new WebSocketClosedException(null, "The web socket is not connected.");
            }

            return connection;
        }

        public async ValueTask ConnectAsync(Uri url, CancellationToken cancellationToken)
        {
            using (await _sendSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            using (await _receiveSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            {
                ThrowIfDisposed();

                _connection?.Dispose();
                _connection = null;
                if (_supportsZLib)
                {
                    _wasLastPayloadZLib = false;
                    _receiveZLibStream?.Dispose();
                    _receiveZLibStream = new ZLibStream(_receiveStream, CompressionMode.Decompress, true);
                }

                var connection = new Connection(_webSocketClientFactory.CreateClient());
                try
                {
                    await connection.WebSocket.ConnectAsync(url, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    connection.Dispose();
                    throw;
                }

                _connection = connection;
            }
        }

        public async ValueTask SendAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            using (await _sendSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            {
                ThrowIfDisposed();

                var connection = GetConnection();
                var webSocket = connection.WebSocket;

                // See Connection.LimboCts for more info on cancellation.
                var sendTask = webSocket.SendAsync(memory, WebSocketMessageType.Text, true, connection.LimboCts.Token).AsTask();
                using (var infiniteCts = Cts.Linked(cancellationToken))
                {
                    var infiniteTask = Task.Delay(Timeout.Infinite, infiniteCts.Token);
                    await Task.WhenAny(infiniteTask, sendTask).ConfigureAwait(false);
                    infiniteCts.Cancel();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                try
                {
                    await sendTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    throw new WebSocketClosedException(webSocket.CloseStatus, webSocket.CloseMessage, ex);
                }
            }
        }

        public async ValueTask<Stream> ReceiveAsync(CancellationToken cancellationToken)
        {
            using (await _receiveSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            {
                ThrowIfDisposed();

                var connection = GetConnection();
                var webSocket = connection.WebSocket;

                // Ensures that the receive stream is fully read and the underlying DeflateStream acknowledges the ZLib suffix.
                if (_supportsZLib && _wasLastPayloadZLib && _receiveStream.Position != _receiveStream.Length)
                {
                    // We just need the inflater to read further so that it picks up the suffix and knows it's done.
                    _ = _receiveZLibStream!.Read(Array.Empty<byte>());
                }

                _receiveStream.Position = 0;
                _receiveStream.SetLength(0);
                do
                {
                    // See Connection.LimboCts for more info on cancellation.
                    var receiveTask = webSocket.ReceiveAsync(_receiveBuffer, connection.LimboCts.Token).AsTask();
                    using (var infiniteCts = Cts.Linked(cancellationToken))
                    {
                        var infiniteTask = Task.Delay(Timeout.Infinite, infiniteCts.Token);
                        await Task.WhenAny(infiniteTask, receiveTask).ConfigureAwait(false);
                        infiniteCts.Cancel();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    WebSocketResult result;
                    try
                    {
                        result = await receiveTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new WebSocketClosedException(webSocket.CloseStatus, webSocket.CloseMessage, ex);
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        var closeStatus = webSocket.CloseStatus;
                        var closeMessage = webSocket.CloseMessage;
                        try
                        {
                            await webSocket.CloseOutputAsync(closeStatus.GetValueOrDefault(), closeMessage, default).ConfigureAwait(false);
                        }
                        catch { }

                        throw new WebSocketClosedException(closeStatus, closeMessage);
                    }

                    _receiveStream.Write(_receiveBuffer.AsSpan(0, result.Count));
                    if (!result.EndOfMessage)
                        continue;

                    if (result.MessageType != WebSocketMessageType.Binary)
                    {
                        _wasLastPayloadZLib = false;
                        _receiveStream.Position = 0;
                        return _receiveStream;
                    }

                    _receiveStream.TryGetBuffer(out var streamBuffer);

                    // We check the data for the ZLib flush which marks the end of the actual message.
                    if (streamBuffer.Count < 4 || BinaryPrimitives.ReadUInt32BigEndian(streamBuffer[^4..]) != 0x0000FFFF)
                        continue;

                    _wasLastPayloadZLib = true;
                    _receiveStream.Position = 0;
                    return _receiveZLibStream!;
                }
                while (!cancellationToken.IsCancellationRequested);

                throw new OperationCanceledException(cancellationToken);
            }
        }

        public async ValueTask CloseAsync(int closeStatus, string? closeMessage = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            using (await _sendSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            using (await _receiveSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
            {
                var connection = _connection;
                if (connection == null)
                {
                    return;
                }

                _connection = null;
                if (connection.WebSocket.State != WebSocketState.Aborted)
                {
                    try
                    {
                        await connection.WebSocket.CloseAsync(closeStatus, closeMessage, cancellationToken).ConfigureAwait(false);
                    }
                    catch { }
                }

                connection.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            using (await _sendSemaphore.EnterAsync().ConfigureAwait(false))
            using (await _receiveSemaphore.EnterAsync().ConfigureAwait(false))
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _receiveZLibStream?.Dispose();
                _receiveStream.Dispose();
                _connection?.Dispose();
                _connection = null;
            }
        }

        private sealed class Connection : IDisposable
        {
            public IWebSocketClient WebSocket { get; }

            /// <summary>
            ///     This is a fix for the ClientWebSocket being garbage and aborting itself on a cancelled ReceiveAsync (and possibly SendAsync)
            ///     rendering us unable to close the connection gracefully.
            ///     1. We create an infinite task that runs alongside the Send/ReceiveAsync() tasks and pass it the actual cancellation token just to signal cancellation.
            ///     2. We pass the Send/ReceiveAsync() task an essentially bogus cancellation token that gets cancelled when we close the connection,
            ///        allowing us to gracefully close and then have the websocket abort or whatever as we don't care about the state of it anymore.
            /// </summary>
            public Cts LimboCts { get; }

            public Connection(IWebSocketClient webSocket)
            {
                WebSocket = webSocket;
                LimboCts = new Cts();
            }

            public void Dispose()
            {
                LimboCts.Cancel();
                LimboCts.Dispose();
                WebSocket.Dispose();
            }
        }
    }
}
