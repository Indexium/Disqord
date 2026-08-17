namespace Disqord.AuditLogs;

public interface IAutoModerationMemberTimedOutAuditLog : ITargetedAuditLog<IUser>
{
    string? RuleName { get; }

    AutoModerationRuleTrigger? RuleTrigger { get; }

    Snowflake? ChannelId { get; }
}
