using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientAutoModerationMessageFlaggedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientAutoModerationActionAuditLog(client, guildId, auditLogJsonModel, model), IAutoModerationMessageFlaggedAuditLog;
