namespace Disqord.AuditLogs;

public interface ISoundboardSoundAuditLogChanges
{
    AuditLogChange<string> Name { get; }

    AuditLogChange<double> Volume { get; }

    AuditLogChange<Snowflake?> EmojiId { get; }

    AuditLogChange<string?> EmojiName { get; }

    AuditLogChange<bool> IsAvailable { get; }
}
