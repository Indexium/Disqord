namespace Disqord.AuditLogs;

public interface IAutoModerationMessageFlaggedAuditLog : ITargetedAuditLog<IUser>
{
    string? RuleName { get; }

    AutoModerationRuleTrigger? RuleTrigger { get; }

    Snowflake? ChannelId { get; }
}
