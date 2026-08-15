using System;
using Disqord.Models;
using Qommon;

namespace Disqord.AuditLogs;

public abstract class TransientAutoModerationActionAuditLog : TransientAuditLog
{
    /// <inheritdoc/>
    public IUser? Target
    {
        get
        {
            if (_target == null && AuditLogJsonModel != null)
            {
                var userModel = Array.Find(AuditLogJsonModel.Users, userModel => userModel.Id == TargetId);
                if (userModel != null)
                    _target = new TransientUser(Client, userModel);
            }

            return _target;
        }
    }
    private IUser? _target;

    /// <inheritdoc/>
    public string? RuleName => Model.Options.GetValueOrDefault()?.AutoModerationRuleName.GetValueOrDefault();

    /// <inheritdoc/>
    public AutoModerationRuleTrigger? RuleTrigger
    {
        get
        {
            var options = Model.Options.GetValueOrDefault();
            if (options == null || !options.AutoModerationRuleTriggerType.HasValue)
                return null;

            return options.AutoModerationRuleTriggerType.Value;
        }
    }

    /// <inheritdoc/>
    public Snowflake? ChannelId
    {
        get
        {
            var options = Model.Options.GetValueOrDefault();
            if (options == null || !options.ChannelId.HasValue)
                return null;

            return options.ChannelId.Value;
        }
    }

    protected TransientAutoModerationActionAuditLog(IClient client, Snowflake guildId, AuditLogJsonModel? auditLogJsonModel, AuditLogEntryJsonModel model)
        : base(client, guildId, auditLogJsonModel, model)
    { }
}
