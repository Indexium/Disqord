using System;
using System.Collections.Generic;

namespace Disqord.Bot.Commands.Application;

public interface IApplicationCommandCache : IAsyncDisposable
{
    /// <summary>
    ///     Gets the IDs of the guilds this cache currently holds commands for.
    /// </summary>
    IReadOnlyCollection<Snowflake> GuildIds { get; }

    IApplicationCommandCacheChanges GetChanges(Snowflake? guildId, IEnumerable<LocalApplicationCommand> commands);

    void ApplyChanges(Snowflake? guildId, IApplicationCommandCacheChanges changes, IEnumerable<IApplicationCommand> commands);
}
