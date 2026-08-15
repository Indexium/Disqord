using Qommon;

namespace Disqord.AuditLogs;

public interface ISoundboardSoundAuditLogData
{
    Optional<Snowflake> SoundId { get; }

    Optional<string> Name { get; }

    Optional<double> Volume { get; }

    Optional<Snowflake?> EmojiId { get; }

    Optional<string?> EmojiName { get; }

    Optional<Snowflake> UserId { get; }

    Optional<bool> IsAvailable { get; }
}
