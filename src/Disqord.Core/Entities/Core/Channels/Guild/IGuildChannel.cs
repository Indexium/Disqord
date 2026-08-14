using System.Collections.Generic;

namespace Disqord;

/// <summary>
///     Represents a guild channel.
/// </summary>
public interface IGuildChannel : IChannel, IGuildEntity, IMentionableEntity
{
    /// <summary>
    ///     Gets the position within the guild of this channel.
    /// </summary>
    int Position { get; }

    /// <summary>
    ///     Gets the permission overwrites of this channel.
    /// </summary>
    IReadOnlyList<IOverwrite> Overwrites { get; }

    /// <summary>
    ///     Gets the flags of this channel.
    /// </summary>
    GuildChannelFlags Flags { get; }

    /// <summary>
    ///     Gets whether the metadata of this channel has been obfuscated because the current user cannot view this channel.
    /// </summary>
    /// <remarks>
    ///     Currently as of August 14th 2026 an obfuscated channel only has meaningful
    ///     <see cref="IChannel.Id"/>, <see cref="IChannel.Type"/>,
    ///     <see cref="Position"/>, and <see cref="ICategorizableGuildChannel.CategoryId"/>.
    ///     Every other property returns obfuscated data and must not be relied on.
    /// </remarks>
    /// <seealso href="https://discord.com/developers/docs/resources/channel#channel-object-obfuscated-channels"> Discord documentation </seealso>
    bool IsObfuscated { get; }
}
