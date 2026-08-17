using System;
using System.Collections.Generic;
using Disqord.Models;
using Qommon;

namespace Disqord.AuditLogs;

public class TransientChannelAuditLogData : IChannelAuditLogData
{
    /// <inheritdoc/>
    public Optional<string> Name { get; }

    /// <inheritdoc/>
    public Optional<string?> Topic { get; }

    /// <inheritdoc/>
    public Optional<int> Bitrate { get; }

    /// <inheritdoc/>
    public Optional<int> MemberLimit { get; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IOverwrite>> Overwrites { get; }

    /// <inheritdoc/>
    public Optional<bool> IsAgeRestricted { get; }

    /// <inheritdoc/>
    public Optional<TimeSpan> Slowmode { get; }

    /// <inheritdoc/>
    public Optional<ChannelType> Type { get; }

    /// <inheritdoc/>
    public Optional<string?> Region { get; }

    /// <inheritdoc/>
    public Optional<GuildChannelFlags> Flags { get; }

    /// <inheritdoc/>
    public Optional<VideoQualityMode> VideoQualityMode { get; }

    /// <inheritdoc/>
    public Optional<TimeSpan> DefaultAutomaticArchiveDuration { get; }

    /// <inheritdoc/>
    public Optional<TimeSpan> DefaultThreadSlowmode { get; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IForumTag>> AvailableTags { get; }

    /// <inheritdoc/>
    public Optional<IEmoji?> DefaultReactionEmoji { get; }

    /// <inheritdoc/>
    public Optional<string?> Template { get; }

    public TransientChannelAuditLogData(IClient client, AuditLogEntryJsonModel model, bool isCreated)
    {
        var changes = new TransientChannelAuditLogChanges(client, model);
        if (isCreated)
        {
            Name = changes.Name.NewValue;
            Topic = changes.Topic.NewValue;
            Bitrate = changes.Bitrate.NewValue;
            MemberLimit = changes.MemberLimit.NewValue;
            Overwrites = changes.Overwrites.NewValue;
            IsAgeRestricted = changes.IsAgeRestricted.NewValue;
            Slowmode = changes.Slowmode.NewValue;
            Type = changes.Type.NewValue;
            Region = changes.Region.NewValue;
            Flags = changes.Flags.NewValue;
            VideoQualityMode = changes.VideoQualityMode.NewValue;
            DefaultAutomaticArchiveDuration = changes.DefaultAutomaticArchiveDuration.NewValue;
            DefaultThreadSlowmode = changes.DefaultThreadSlowmode.NewValue;
            AvailableTags = changes.AvailableTags.NewValue;
            DefaultReactionEmoji = changes.DefaultReactionEmoji.NewValue;
            Template = changes.Template.NewValue;
        }
        else
        {
            Name = changes.Name.OldValue;
            Topic = changes.Topic.OldValue;
            Bitrate = changes.Bitrate.OldValue;
            MemberLimit = changes.MemberLimit.OldValue;
            Overwrites = changes.Overwrites.OldValue;
            IsAgeRestricted = changes.IsAgeRestricted.OldValue;
            Slowmode = changes.Slowmode.OldValue;
            Type = changes.Type.OldValue;
            Region = changes.Region.OldValue;
            Flags = changes.Flags.OldValue;
            VideoQualityMode = changes.VideoQualityMode.OldValue;
            DefaultAutomaticArchiveDuration = changes.DefaultAutomaticArchiveDuration.OldValue;
            DefaultThreadSlowmode = changes.DefaultThreadSlowmode.OldValue;
            AvailableTags = changes.AvailableTags.OldValue;
            DefaultReactionEmoji = changes.DefaultReactionEmoji.OldValue;
            Template = changes.Template.OldValue;
        }
    }
}
