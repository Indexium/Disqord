using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Gateway.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qommon;

namespace Disqord.Api;

/// <summary>
///     Represents the default, local shard coordinator implementation.
/// </summary>
public class LocalDiscordShardCoordinator : DiscordShardCoordinator
{
    /// <inheritdoc/>
    /// <returns>
    ///     <see langword="true"/> if <see cref="CustomShardSet"/> is not set.
    /// </returns>
    public override bool HasDynamicShardSets => CustomShardSet.Equals(default);

    /// <summary>
    ///     Gets the custom shard set.
    /// </summary>
    /// <remarks>
    ///     Defaults to an empty instance.
    /// </remarks>
    protected ShardSet CustomShardSet { get; }

    /// <summary>
    ///     Gets or sets the semaphore.
    /// </summary>
    protected SemaphoreSlim? IdentifySemaphore { get; set; }

    /// <summary>
    ///     Gets or sets the semaphore reset timer.
    /// </summary>
    protected Timer? IdentifySemaphoreResetTimer { get; set; }

    public LocalDiscordShardCoordinator(
        IOptions<LocalDiscordShardCoordinatorConfiguration> options,
        ILogger<LocalDiscordShardCoordinator> logger)
        : base(logger)
    {
        var configuration = options.Value;
        CustomShardSet = configuration.CustomShardSet.GetValueOrDefault();
    }

    /// <inheritdoc/>
    protected override async ValueTask<ShardSet> OnGetShardSet(CancellationToken stoppingToken)
    {
        ShardSet currentShardSet;
        if (CustomShardSet.Equals(default))
        {
            currentShardSet = await base.OnGetShardSet(stoppingToken).ConfigureAwait(false);
            Logger.LogInformation("Using Discord's recommended total shard amount of {ShardCount}.", currentShardSet.ShardIds.Count);
        }
        else
        {
            currentShardSet = CustomShardSet;
            Logger.LogInformation("Using a custom total shard amount of {ShardCount}.", currentShardSet.ShardIds.Count);
        }

        lock (this)
        {
            CurrentShardSet = currentShardSet;
            IdentifySemaphore = new SemaphoreSlim(currentShardSet.MaxConcurrency, currentShardSet.MaxConcurrency);
        }

        return currentShardSet;
    }

    /// <inheritdoc/>
    public override ValueTask WaitToIdentifyShardAsync(ShardId shardId, CancellationToken stoppingToken)
    {
        lock (this)
        {
            var currentShardSet = CurrentShardSet;
            var semaphore = IdentifySemaphore;
            Guard.IsNotDefault(currentShardSet);
            Guard.IsNotNull(semaphore);

            return new(semaphore.WaitAsync(stoppingToken));
        }
    }

    /// <inheritdoc/>
    public override ValueTask OnShardSetInvalidated(CancellationToken stoppingToken)
    {
        lock (this)
        {
            IdentifySemaphore = null;
            IdentifySemaphoreResetTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        return base.OnShardSetInvalidated(stoppingToken);
    }

    /// <inheritdoc/>
    public override ValueTask OnShardReady(ShardId shardId, string sessionId, CancellationToken stoppingToken)
    {
        lock (this)
        {
            if (IdentifySemaphoreResetTimer == null)
            {
                IdentifySemaphoreResetTimer = new Timer(state =>
                {
                    var @this = Unsafe.As<LocalDiscordShardCoordinator>(state!);
                    try
                    {
                        lock (@this)
                        {
                            if (@this.IdentifySemaphore != null)
                            {
                                var releaseCount = @this.CurrentShardSet.MaxConcurrency - @this.IdentifySemaphore.CurrentCount;
                                if (releaseCount >= 1)
                                {
                                    @this.IdentifySemaphore.Release(releaseCount);
                                }
                            }

                            if (@this.IdentifySemaphoreResetTimer != null)
                            {
                                @this.IdentifySemaphoreResetTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        @this.Logger.LogError(ex, "An exception occurred in the coordinator semaphore timer.");
                    }
                }, this, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            }
            else
            {
                IdentifySemaphoreResetTimer.Change(TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            }
        }

        return default;
    }
}
