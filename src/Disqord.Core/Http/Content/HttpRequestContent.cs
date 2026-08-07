using System;
using System.Threading;
using System.Threading.Tasks;

namespace Disqord.Http;

/// <summary>
///     Represents HTTP content.
/// </summary>
public abstract class HttpRequestContent : HeadersBase, IDisposable
{
    /// <summary>
    ///     Resets this content so it can be sent again, e.g. after a rate-limit retry.
    /// </summary>
    /// <remarks>
    ///     This is called before every send, including the first one.
    ///     The default implementation does nothing, which is correct for content backed by immutable data.
    /// </remarks>
    /// <param name="cancellationToken"> The cancellation token. </param>
    public virtual ValueTask RewindAsync(CancellationToken cancellationToken)
    {
        return default;
    }

    /// <summary>
    ///     Disposes the resources held by this content.
    /// </summary>
    public virtual void Dispose()
    { }
}