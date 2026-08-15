using System;

namespace Disqord.AuditLogs;

public interface IWebhookAuditLogChanges
{
    AuditLogChange<string> Name { get; }

    [Obsolete("This value is never present on update entries.")]
    AuditLogChange<WebhookType> Type { get; }

    AuditLogChange<string?> AvatarHash { get; }

    AuditLogChange<Snowflake> ChannelId { get; }

    [Obsolete("This value is never present on update entries.")]
    AuditLogChange<Snowflake?> ApplicationId { get; }
}
