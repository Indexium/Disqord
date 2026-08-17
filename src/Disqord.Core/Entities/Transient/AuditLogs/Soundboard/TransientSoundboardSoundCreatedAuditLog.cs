using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientSoundboardSoundCreatedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientDataAuditLog<ISoundboardSoundAuditLogData>(client, guildId, auditLogJsonModel, model), ISoundboardSoundCreatedAuditLog
{
    /// <inheritdoc/>
    public override ISoundboardSoundAuditLogData Data { get; } = new TransientSoundboardSoundAuditLogData(client, model, true);
}
