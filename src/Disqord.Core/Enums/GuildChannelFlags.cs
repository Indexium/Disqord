using System;

namespace Disqord;

/// <summary>
///     Represents the flags of a guild channel.
/// </summary>
[Flags]
public enum GuildChannelFlags
{
    /// <summary>
    ///     The thread channel is pinned to the top of its parent forum channel.
    /// </summary>
    Pinned = 1 << 1,

    /// <summary>
    ///     The forum channel requires a tag to be specified for threads created in it.
    /// </summary>
    RequiresTag = 1 << 4,

    /// <summary>
    ///     The media channel hides the embedded media download options for media in the threads created in it.
    /// </summary>
    HideMediaDownloadOptions = 1 << 15,

    /// <summary>
    ///     The metadata of the channel has been obfuscated because the current user cannot view it.
    /// </summary>
    /// <seealso href="https://discord.com/developers/docs/resources/channel#channel-object-obfuscated-channels"> Discord documentation </seealso>
    Obfuscated = 1 << 17,

    /// <summary>
    ///     The channel is marked as a spoiler.
    /// </summary>
    Spoiler = 1 << 21,
}
