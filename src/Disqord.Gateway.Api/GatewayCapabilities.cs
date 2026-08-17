using System;

namespace Disqord.Gateway;

/// <summary>
///     Represents Discord gateway capabilities.
/// </summary>
/// <seealso href="https://discord.com/developers/docs/events/gateway-events#identify-gateway-capabilities"> Discord documentation </seealso>
[Flags]
public enum GatewayCapabilities : ulong
{
    /// <summary>
    ///     No capability specified.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Opts into receiving obfuscated metadata for guild channels the bot cannot view instead of the full channel data.
    /// </summary>
    /// <remarks>
    ///     This is a temporary, testing-only opt-in.
    ///     Discord plans to obfuscate channels for all bots regardless of this capability.
    /// </remarks>
    /// <seealso href="https://discord.com/developers/docs/resources/channel#channel-object-obfuscated-channels"> Discord documentation </seealso>
    ChannelObfuscation = 1 << 15,
}
