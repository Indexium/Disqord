using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientSoundboardSoundDeletedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientDataAuditLog<ISoundboardSoundAuditLogData>(client, guildId, auditLogJsonModel, model), ISoundboardSoundDeletedAuditLog
{
    /// <inheritdoc/>
    public override ISoundboardSoundAuditLogData Data { get; } = new TransientSoundboardSoundAuditLogData(client, model, false);
}
