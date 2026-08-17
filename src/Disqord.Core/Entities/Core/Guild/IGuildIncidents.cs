using System;

namespace Disqord;

/// <summary>
///     Represents the security incidents data of a guild.
/// </summary>
public interface IGuildIncidents
{
    /// <summary>
    ///     Gets when invites are disabled until.
    /// </summary>
    DateTimeOffset? InvitesDisabledUntil { get; }

    /// <summary>
    ///     Gets when direct messages are disabled until.
    /// </summary>
    DateTimeOffset? DirectMessagesDisabledUntil { get; }

    /// <summary>
    ///     Gets when the most recent direct message spam was detected.
    /// </summary>
    DateTimeOffset? DirectMessageSpamDetectedAt { get; }

    /// <summary>
    ///     Gets when the most recent raid was detected.
    /// </summary>
    DateTimeOffset? RaidDetectedAt { get; }
}
