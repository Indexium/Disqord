using Disqord.Models;

namespace Disqord.AuditLogs;

public class TransientAutoModerationMemberTimedOutAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
    : TransientAutoModerationActionAuditLog(client, guildId, auditLogJsonModel, model), IAutoModerationMemberTimedOutAuditLog;
