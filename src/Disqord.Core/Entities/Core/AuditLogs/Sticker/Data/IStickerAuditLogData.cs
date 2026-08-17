using System;
using Qommon;

namespace Disqord.AuditLogs;

public interface IStickerAuditLogData
{
    Optional<string> Name { get; }

    Optional<string> Description { get; }

    Optional<string> Tags { get; }

    Optional<StickerFormatType> FormatType { get; }

    Optional<bool> IsAvailable { get; }

    [Obsolete("Use IAuditLog.GuildId instead.")]
    Optional<Snowflake> GuildId { get; }
}