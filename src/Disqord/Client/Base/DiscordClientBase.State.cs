﻿using System.Collections.Generic;

namespace Disqord
{
    public abstract partial class DiscordClientBase
    {
        /// <summary>
        ///     Gets the currently logged-in user.
        /// </summary>
        public CachedCurrentUser CurrentUser => State._currentUser;

        /// <summary>
        ///     Gets the guilds for this client.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, CachedGuild> Guilds { get; }

        /// <summary>
        ///     Gets the users for this client.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, CachedUser> Users { get; }

        /// <summary>
        ///     Gets the private channels for this client.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, CachedPrivateChannel> PrivateChannels { get; }

        /// <summary>
        ///     Gets the DM channels for this client.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, CachedDmChannel> DmChannels { get; }

        /// <summary>
        ///     Gets the group DM channels for this client.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, CachedGroupChannel> GroupChannels { get; }

        internal DiscordClientState State { get; }

        public CachedUserMessage GetMessage(Snowflake channelId, Snowflake messageId)
            => State.GetMessage(channelId, messageId);

        public IReadOnlyList<CachedUserMessage> GetMessages(Snowflake channelId)
            => State.GetMessages(channelId);

        public CachedGuild GetGuild(Snowflake id)
            => State.GetGuild(id);

        public CachedGuildChannel GetGuildChannel(Snowflake id)
            => State.GetGuildChannel(id);

        /// <summary>
        ///     Looks up the user cache for the <see cref="CachedUser"/> with the given id.
        /// </summary>
        /// <param name="id"> The id of the user. </param>
        /// <returns>
        ///     The found <see cref="CachedUser"/> or <see langword="null"/>.
        /// </returns>
        public CachedUser GetUser(Snowflake id)
            => State.GetUser(id);

        public CachedPrivateChannel GetPrivateChannel(Snowflake id)
            => State.GetPrivateChannel(id);

        public CachedChannel GetChannel(Snowflake id)
            => State.GetChannel(id);
    }
}