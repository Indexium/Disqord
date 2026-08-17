using System;

namespace Disqord.Gateway;

public class CurrentThreadMemberUpdatedEventArgs : EventArgs
{
    /// <summary>
    ///     Gets the ID of the guild in which the update occurred.
    /// </summary>
    public Snowflake GuildId { get; }

    /// <summary>
    ///     Gets the ID of the thread that the update occurred for.
    /// </summary>
    public Snowflake ThreadId { get; }

    /// <summary>
    ///     Gets the cached thread that the update occurred for.
    ///     Returns <see langword="null"/> if the thread was not cached.
    /// </summary>
    public CachedThreadChannel? Thread { get; }

    /// <summary>
    ///     Gets the updated thread member of the current user.
    /// </summary>
    public IThreadMember Member { get; }

    public CurrentThreadMemberUpdatedEventArgs(
        Snowflake guildId,
        Snowflake threadId,
        CachedThreadChannel? thread,
        IThreadMember member)
    {
        GuildId = guildId;
        ThreadId = threadId;
        Thread = thread;
        Member = member;
    }
}
