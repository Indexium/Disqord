using System;

namespace Disqord;

/// <summary>
///     Represents the flags of an invite.
/// </summary>
[Flags]
public enum InviteFlags
{
    /// <summary>
    ///     The invite is a guest invite for a voice channel.
    /// </summary>
    GuestInvite = 1 << 0,
}
