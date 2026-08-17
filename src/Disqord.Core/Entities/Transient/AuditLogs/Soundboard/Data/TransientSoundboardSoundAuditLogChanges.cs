using Disqord.Models;
using Microsoft.Extensions.Logging;

namespace Disqord.AuditLogs;

public class TransientSoundboardSoundAuditLogChanges : ISoundboardSoundAuditLogChanges
{
    internal AuditLogChange<Snowflake> SoundId { get; }

    internal AuditLogChange<Snowflake> UserId { get; }

    /// <inheritdoc/>
    public AuditLogChange<string> Name { get; }

    /// <inheritdoc/>
    public AuditLogChange<double> Volume { get; }

    /// <inheritdoc/>
    public AuditLogChange<Snowflake?> EmojiId { get; }

    /// <inheritdoc/>
    public AuditLogChange<string?> EmojiName { get; }

    /// <inheritdoc/>
    public AuditLogChange<bool> IsAvailable { get; }

    public TransientSoundboardSoundAuditLogChanges(IClient client, AuditLogEntryJsonModel model)
    {
        for (var i = 0; i < model.Changes.Value.Length; i++)
        {
            var change = model.Changes.Value[i];
            switch (change.Key)
            {
                case "sound_id":
                case "id":
                {
                    SoundId = AuditLogChange<Snowflake>.Convert(change);
                    break;
                }
                case "name":
                {
                    Name = AuditLogChange<string>.Convert(change);
                    break;
                }
                case "volume":
                {
                    Volume = AuditLogChange<double>.Convert(change);
                    break;
                }
                case "emoji_id":
                {
                    EmojiId = AuditLogChange<Snowflake?>.Convert(change);
                    break;
                }
                case "emoji_name":
                {
                    EmojiName = AuditLogChange<string?>.Convert(change);
                    break;
                }
                case "user_id":
                {
                    UserId = AuditLogChange<Snowflake>.Convert(change);
                    break;
                }
                case "available":
                {
                    IsAvailable = AuditLogChange<bool>.Convert(change);
                    break;
                }
                case "guild_id":
                {
                    break;
                }
                default:
                {
                    client.Logger.LogDebug("Unknown key {0} for {1}", change.Key, this);
                    break;
                }
            }
        }
    }
}
