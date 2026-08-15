using System;
using Disqord.Models;
using Qommon;

namespace Disqord.AuditLogs;

public class TransientThreadAuditLogData : IThreadAuditLogData
{
    /// <inheritdoc/>
    public Optional<string> Name { get; }

    /// <inheritdoc/>
    public Optional<bool> IsArchived { get; }

    /// <inheritdoc/>
    public Optional<bool> IsLocked { get; }

    /// <inheritdoc/>
    public Optional<TimeSpan> AutomaticArchiveDuration { get; }

    /// <inheritdoc/>
    public Optional<TimeSpan> Slowmode { get; }

    /// <inheritdoc/>
    public Optional<ChannelType> Type { get; }

    /// <inheritdoc/>
    public Optional<bool> AllowsInvitation { get; }

    /// <inheritdoc/>
    public Optional<GuildChannelFlags> Flags { get; }

    public TransientThreadAuditLogData(IClient client, AuditLogEntryJsonModel model, bool isCreated)
    {
        var changes = new TransientThreadAuditLogChanges(client, model);
        if (isCreated)
        {
            Name = changes.Name.NewValue;
            IsArchived = changes.IsArchived.NewValue;
            IsLocked = changes.IsLocked.NewValue;
            AutomaticArchiveDuration = changes.AutomaticArchiveDuration.NewValue;
            Slowmode = changes.Slowmode.NewValue;
            Type = changes.Type.NewValue;
            AllowsInvitation = changes.AllowsInvitation.NewValue;
            Flags = changes.Flags.NewValue;
        }
        else
        {
            Name = changes.Name.OldValue;
            IsArchived = changes.IsArchived.OldValue;
            IsLocked = changes.IsLocked.OldValue;
            AutomaticArchiveDuration = changes.AutomaticArchiveDuration.OldValue;
            Slowmode = changes.Slowmode.OldValue;
            Type = changes.Type.OldValue;
            AllowsInvitation = changes.AllowsInvitation.OldValue;
            Flags = changes.Flags.OldValue;
        }
    }
}
