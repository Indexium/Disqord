using Disqord.Models;
using Qommon;

namespace Disqord.AuditLogs;

public class TransientVoiceChannelStatusCreatedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientAuditLog(client, guildId, auditLogJsonModel, model), IVoiceChannelStatusCreatedAuditLog
{
    /// <inheritdoc/>
    public string? Status => Model.Options.GetValueOrDefault()?.Status.GetValueOrDefault();
}
