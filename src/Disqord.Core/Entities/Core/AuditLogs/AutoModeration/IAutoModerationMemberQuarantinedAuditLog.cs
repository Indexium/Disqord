namespace Disqord.AuditLogs;

public interface IAutoModerationMemberQuarantinedAuditLog : ITargetedAuditLog<IUser>
{
    string? RuleName { get; }

    AutoModerationRuleTrigger? RuleTrigger { get; }

    Snowflake? ChannelId { get; }
}
