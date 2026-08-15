using System;
using System.Collections.Generic;

namespace Disqord.AuditLogs;

public interface IChannelAuditLogChanges
{
    AuditLogChange<string> Name { get; }

    AuditLogChange<string?> Topic { get; }

    AuditLogChange<int> Bitrate { get; }

    AuditLogChange<int> MemberLimit { get; }

    AuditLogChange<IReadOnlyList<IOverwrite>> Overwrites { get; }

    AuditLogChange<bool> IsAgeRestricted { get; }

    AuditLogChange<TimeSpan> Slowmode { get; }

    AuditLogChange<ChannelType> Type { get; }

    AuditLogChange<string?> Region { get; }

    AuditLogChange<GuildChannelFlags> Flags { get; }

    AuditLogChange<VideoQualityMode> VideoQualityMode { get; }

    AuditLogChange<TimeSpan> DefaultAutomaticArchiveDuration { get; }

    AuditLogChange<TimeSpan> DefaultThreadSlowmode { get; }

    AuditLogChange<IReadOnlyList<IForumTag>> AvailableTags { get; }

    AuditLogChange<IEmoji?> DefaultReactionEmoji { get; }

    AuditLogChange<string?> Template { get; }
}
