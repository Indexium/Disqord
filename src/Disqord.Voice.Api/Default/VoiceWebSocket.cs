using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Utilities.Threading;
using Disqord.WebSocket;
using Microsoft.Extensions.Logging;
using Qommon.Threading;

namespace Disqord.Voice.Api.Default;

internal sealed class VoiceWebSocket(
    ILogger logger,
    IWebSocketClientFactory webSocketClientFactory) : IAsyncDisposable
{
    public const int ReceiveBufferSize = 8192;

    public ILogger Logger { get; } = logger;

    private Connection? _connection;

    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);

    private readonly SemaphoreSlim _receiveSemaphore = new(1, 1);
    private readonly byte[] _receiveBuffer = new byte[ReceiveBufferSize];
    private readonly MemoryStream _receiveStream = new(ReceiveBufferSize * 2);

    private bool _isDisposed;

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(null, "The voice web socket client has been disposed.");
        }
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

            var connection = new Connection(webSocketClientFactory.CreateClient());
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

    public async ValueTask SendAsync(ReadOnlyMemory<byte> memory, WebSocketMessageType messageType, CancellationToken cancellationToken)
    {
        using (await _sendSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            ThrowIfDisposed();

            var connection = GetConnection();
            var webSocket = connection.WebSocket;

            // See Connection.LimboCts for more info on cancellation.
            var sendTask = webSocket.SendAsync(memory, messageType, true, connection.LimboCts.Token).AsTask();
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

    public async ValueTask<(MemoryStream Stream, bool IsBinary)> ReceiveAsync(CancellationToken cancellationToken)
    {
        using (await _receiveSemaphore.EnterAsync(cancellationToken).ConfigureAwait(false))
        {
            ThrowIfDisposed();

            var connection = GetConnection();
            var webSocket = connection.WebSocket;

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

                cancellationToken.ThrowIfCancellationRequested();

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

                _receiveStream.Position = 0;
                return (_receiveStream, result.MessageType == WebSocketMessageType.Binary);
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
