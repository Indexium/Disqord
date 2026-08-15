using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientSoundboardSoundUpdatedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientChangesAuditLog<ISoundboardSoundAuditLogChanges>(client, guildId, auditLogJsonModel, model), ISoundboardSoundUpdatedAuditLog
{
    /// <inheritdoc/>
    public override ISoundboardSoundAuditLogChanges Changes { get; } = new TransientSoundboardSoundAuditLogChanges(client, model);
}
