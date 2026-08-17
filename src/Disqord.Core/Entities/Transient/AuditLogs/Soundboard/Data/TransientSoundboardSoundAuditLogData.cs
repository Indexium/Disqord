using Disqord.Models;
using Qommon;

namespace Disqord.AuditLogs;

public class TransientSoundboardSoundAuditLogData : ISoundboardSoundAuditLogData
{
    /// <inheritdoc/>
    public Optional<Snowflake> SoundId { get; }

    /// <inheritdoc/>
    public Optional<string> Name { get; }

    /// <inheritdoc/>
    public Optional<double> Volume { get; }

    /// <inheritdoc/>
    public Optional<Snowflake?> EmojiId { get; }

    /// <inheritdoc/>
    public Optional<string?> EmojiName { get; }

    /// <inheritdoc/>
    public Optional<Snowflake> UserId { get; }

    /// <inheritdoc/>
    public Optional<bool> IsAvailable { get; }

    public TransientSoundboardSoundAuditLogData(IClient client, AuditLogEntryJsonModel model, bool isCreated)
    {
        var changes = new TransientSoundboardSoundAuditLogChanges(client, model);
        if (isCreated)
        {
            SoundId = changes.SoundId.NewValue;
            Name = changes.Name.NewValue;
            Volume = changes.Volume.NewValue;
            EmojiId = changes.EmojiId.NewValue;
            EmojiName = changes.EmojiName.NewValue;
            UserId = changes.UserId.NewValue;
            IsAvailable = changes.IsAvailable.NewValue;
        }
        else
        {
            SoundId = changes.SoundId.OldValue;
            Name = changes.Name.OldValue;
            Volume = changes.Volume.OldValue;
            EmojiId = changes.EmojiId.OldValue;
            EmojiName = changes.EmojiName.OldValue;
            UserId = changes.UserId.OldValue;
            IsAvailable = changes.IsAvailable.OldValue;
        }
    }
}
