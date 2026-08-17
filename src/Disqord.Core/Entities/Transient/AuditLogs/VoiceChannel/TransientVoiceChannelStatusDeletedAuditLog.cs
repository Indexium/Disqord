using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientVoiceChannelStatusDeletedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientAuditLog(client, guildId, auditLogJsonModel, model), IVoiceChannelStatusDeletedAuditLog;
