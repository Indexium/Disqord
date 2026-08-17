using System;

namespace Disqord.AuditLogs;

public interface IStickerAuditLogChanges
{
    AuditLogChange<string> Name { get; }

    AuditLogChange<string> Description { get; }

    AuditLogChange<string> Tags { get; }

    [Obsolete("This value is never present on update entries.")]
    AuditLogChange<StickerFormatType> FormatType { get; }

    AuditLogChange<bool> IsAvailable { get; }

    [Obsolete("Use IAuditLog.GuildId instead.")]
    AuditLogChange<Snowflake> GuildId { get; }
}