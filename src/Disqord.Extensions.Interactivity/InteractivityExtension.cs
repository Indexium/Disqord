﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qommon;
using Qommon.Collections.ThreadSafe;

namespace Disqord.Extensions.Interactivity;

public class InteractivityExtension : DiscordClientExtension
{
    /// <summary>
    ///     Gets the default timeout used for waiting for events.
    /// </summary>
    public TimeSpan DefaultWaitTimeout { get; }

    /// <summary>
    ///     Gets the default timeout used for menus.
    /// </summary>
    public TimeSpan DefaultMenuTimeout { get; }

    // ChannelId -> Waiters
    private readonly IThreadSafeDictionary<Snowflake, LinkedList<Waiter<InteractionReceivedEventArgs>>> _interactionWaiters;

    // ChannelId -> Waiters
    private readonly IThreadSafeDictionary<Snowflake, LinkedList<Waiter<MessageReceivedEventArgs>>> _messageWaiters;

    // MessageId -> Waiters
    private readonly IThreadSafeDictionary<Snowflake, LinkedList<Waiter<ReactionAddedEventArgs>>> _reactionWaiters;

    // MessageId -> Menu
    private readonly IThreadSafeDictionary<Snowflake, MenuBase> _menus;

    public InteractivityExtension(
        IOptions<InteractivityExtensionConfiguration> options,
        ILogger<InteractivityExtension> logger)
        : base(logger)
    {
        var configuration = options.Value;
        DefaultWaitTimeout = configuration.DefaultWaitTimeout;
        DefaultMenuTimeout = configuration.DefaultMenuTimeout;

        _interactionWaiters = ThreadSafeDictionary.Monitor.Create<Snowflake, LinkedList<Waiter<InteractionReceivedEventArgs>>>();
        _messageWaiters = ThreadSafeDictionary.Monitor.Create<Snowflake, LinkedList<Waiter<MessageReceivedEventArgs>>>();
        _reactionWaiters = ThreadSafeDictionary.Monitor.Create<Snowflake, LinkedList<Waiter<ReactionAddedEventArgs>>>();
        _menus = ThreadSafeDictionary.Monitor.Create<Snowflake, MenuBase>();
    }

    /// <inheritdoc/>
    protected override ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        Client.InteractionReceived += InteractionReceivedAsync;
        Client.MessageReceived += MessageReceivedAsync;
        Client.ReactionAdded += ReactionAddedAsync;

        return default;
    }

    /// <summary>
    ///     Waits for an interaction in the channel with the specified <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId"> The ID of the channel to wait for the interaction in. </param>
    /// <param name="predicate"> The predicate to filter the interactions with. </param>
    /// <param name="timeout"> The timeout of the wait. </param>
    /// <param name="cancellationToken"> The cancellation token to observe. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> representing the wait with the result being the matching interaction event data
    ///     or <see langword="null"/> if the wait timed out.
    /// </returns>
    public Task<InteractionReceivedEventArgs?> WaitForInteractionAsync(
        Snowflake channelId, Predicate<InteractionReceivedEventArgs>? predicate = null,
        TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        return WaitForEventAsync(_interactionWaiters, channelId, predicate, timeout, cancellationToken);
    }

    /// <summary>
    ///     Waits for a message in the channel with the specified <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId"> The ID of the channel to wait for the message in. </param>
    /// <param name="predicate"> The predicate to filter the messages with. </param>
    /// <param name="timeout"> The timeout of the wait. </param>
    /// <param name="cancellationToken"> The cancellation token to observe. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> representing the wait with the result being the matching message event data
    ///     or <see langword="null"/> if the wait timed out.
    /// </returns>
    public Task<MessageReceivedEventArgs?> WaitForMessageAsync(
        Snowflake channelId, Predicate<MessageReceivedEventArgs>? predicate = null,
        TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        return WaitForEventAsync(_messageWaiters, channelId, predicate, timeout, cancellationToken);
    }

    /// <summary>
    ///     Waits for a reaction on the message with the specified <paramref name="messageId"/>.
    /// </summary>
    /// <param name="messageId"> The ID of the message to wait for the reaction on. </param>
    /// <param name="predicate"> The predicate to filter the reactions with. </param>
    /// <param name="timeout"> The timeout of the wait. </param>
    /// <param name="cancellationToken"> The cancellation token to observe. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> representing the wait with the result being the matching reaction event data
    ///     or <see langword="null"/> if the wait timed out.
    /// </returns>
    public Task<ReactionAddedEventArgs?> WaitForReactionAsync(
        Snowflake messageId, Predicate<ReactionAddedEventArgs>? predicate = null,
        TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        return WaitForEventAsync(_reactionWaiters, messageId, predicate, timeout, cancellationToken);
    }

    private async Task<TEventArgs?> WaitForEventAsync<TEventArgs>(IThreadSafeDictionary<Snowflake, LinkedList<Waiter<TEventArgs>>> eventWaiters,
        Snowflake entityId, Predicate<TEventArgs>? predicate,
        TimeSpan timeout, CancellationToken cancellationToken)
        where TEventArgs : EventArgs
    {
        cancellationToken.ThrowIfCancellationRequested();

        timeout = timeout != default
            ? timeout
            : DefaultWaitTimeout;

        using (var cts = Cts.Linked(Client.StoppingToken, cancellationToken))
        using (var waiter = new Waiter<TEventArgs>(predicate, timeout, cts.Token))
        {
            var waiters = eventWaiters.GetOrAdd(entityId, _ => new LinkedList<Waiter<TEventArgs>>());
            lock (waiters)
            {
                waiters.AddLast(waiter);
            }

            var task = waiter.Task;
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken != cts.Token)
            {
                return null;
            }
            finally
            {
                if (task.IsCanceled)
                {
                    lock (waiters)
                    {
                        waiters.Remove(waiter);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Starts the menu in the channel with the specified <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId"> The ID of the channel to start the menu in. </param>
    /// <param name="menu"> The menu to start. </param>
    /// <param name="timeout"> The timeout of the menu. </param>
    /// <param name="cancellationToken"> The cancellation token to observe. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> representing the start.
    /// </returns>
    public async Task StartMenuAsync(Snowflake channelId, MenuBase menu,
        TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        await InternalStartMenuAsync(channelId, menu, timeout, cancellationToken).ConfigureAwait(false);
        _ = RunMenuAsync(channelId, menu, timeout, cancellationToken);
    }

    /// <summary>
    ///     Runs the menu in the channel with the specified <paramref name="channelId"/>,
    ///     i.e. starts it, if it has not been started yet, and waits for the menu to stop.
    /// </summary>
    /// <param name="channelId"> The ID of the channel to start the menu in if the menu is not running. </param>
    /// <param name="menu"> The menu to run. </param>
    /// <param name="timeout"> The timeout of the menu. </param>
    /// <param name="cancellationToken"> The cancellation token to observe. </param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> representing the run.
    /// </returns>
    public async Task RunMenuAsync(Snowflake channelId, MenuBase menu,
        TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(menu);

        if (!menu.IsRunning)
            await InternalStartMenuAsync(channelId, menu, timeout, cancellationToken).ConfigureAwait(false);

        try
        {
            await using (menu.ConfigureAwait(false))
            {
                await menu.Task.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken != cancellationToken)
        { }
        finally
        {
            _menus.Remove(menu.MessageId);
        }
    }

    private async Task InternalStartMenuAsync(Snowflake channelId, MenuBase menu,
        TimeSpan timeout, CancellationToken cancellationToken)
    {
        timeout = timeout != default
            ? timeout
            : DefaultMenuTimeout;

        menu.Interactivity = this;
        menu.ChannelId = channelId;
        try
        {
            menu.MessageId = await menu.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An exception occurred while attempting to initialize menu {menu.GetType()}.", ex);
        }

        if (!_menus.TryAdd(menu.MessageId, menu))
            throw new InvalidOperationException($"A menu for the message ID {menu.MessageId} has already been added.");

        try
        {
            menu.Start(timeout, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An exception occurred while attempting to start menu {menu.GetType()}.", ex);
        }
    }

    private async Task InteractionReceivedAsync(object? sender, InteractionReceivedEventArgs e)
    {
        await Task.Yield();

        if (e.Interaction is IComponentInteraction interaction && _menus.TryGetValue(interaction.Message.Id, out var menu))
        {
            await menu.OnInteractionReceived(e).ConfigureAwait(false);
        }
        else
        {
            CompleteWaiters(_interactionWaiters, e.ChannelId, e);
        }
    }

    private async Task MessageReceivedAsync(object? sender, MessageReceivedEventArgs e)
    {
        await Task.Yield();
        CompleteWaiters(_messageWaiters, e.ChannelId, e);
    }

    private async Task ReactionAddedAsync(object? sender, ReactionAddedEventArgs e)
    {
        await Task.Yield();
        CompleteWaiters(_reactionWaiters, e.MessageId, e);
    }

    private static void CompleteWaiters<TEventArgs>(IThreadSafeDictionary<Snowflake, LinkedList<Waiter<TEventArgs>>> eventWaiters,
        Snowflake entityId, TEventArgs e)
        where TEventArgs : EventArgs
    {
        if (!eventWaiters.TryGetValue(entityId, out var waiters))
            return;

        lock (waiters)
        {
            for (var current = waiters.First; current != null;)
            {
                if (current.Value.TryComplete(e))
                {
                    var next = current.Next;
                    waiters.Remove(current);
                    current = next;
                    continue;
                }

                current = current.Next;
            }
        }
    }
}

internal class Waiter<TEventArgs> : IDisposable
    where TEventArgs : EventArgs
{
    public Task<TEventArgs> Task => _tcs.Task;

    private readonly Predicate<TEventArgs>[]? _predicates;
    private readonly Tcs<TEventArgs> _tcs;

    private readonly Timer? _timeoutTimer;
    private readonly CancellationTokenRegistration _reg;

    public Waiter(Predicate<TEventArgs>? predicate, TimeSpan timeout, CancellationToken cancellationToken)
    {
        _predicates = Unsafe.As<Predicate<TEventArgs>[]>(predicate?.GetInvocationList());
        _tcs = new Tcs<TEventArgs>();

        _reg = cancellationToken.UnsafeRegister(CancellationCallback, _tcs);

        if (timeout != Timeout.InfiniteTimeSpan)
        {
            _timeoutTimer = new Timer(TimerCallback, _tcs, timeout, Timeout.InfiniteTimeSpan);
        }

        static void TimerCallback(object? state)
        {
            var tcs = Unsafe.As<Tcs<TEventArgs>>(state)!;
            tcs.Cancel(new CancellationToken(true));
        }

        static void CancellationCallback(object? state, CancellationToken cancellationToken)
        {
            var tcs = Unsafe.As<Tcs<TEventArgs>>(state)!;
            tcs.Cancel(cancellationToken);
        }
    }

    public bool TryComplete(TEventArgs e)
    {
        try
        {
            if (_predicates != null)
            {
                foreach (var predicate in _predicates)
                {
                    if (!predicate(e))
                        return false;
                }
            }
        }
        catch (Exception ex)
        {
            _tcs.Throw(ex);
            return true;
        }

        return _tcs.Complete(e);
    }

    public void Dispose()
    {
        _timeoutTimer?.Dispose();
        _reg.Dispose();
    }
}
