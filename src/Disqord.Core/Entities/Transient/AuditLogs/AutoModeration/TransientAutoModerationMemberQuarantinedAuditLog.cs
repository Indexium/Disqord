using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientAutoModerationMemberQuarantinedAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientAutoModerationActionAuditLog(client, guildId, auditLogJsonModel, model), IAutoModerationMemberQuarantinedAuditLog;
