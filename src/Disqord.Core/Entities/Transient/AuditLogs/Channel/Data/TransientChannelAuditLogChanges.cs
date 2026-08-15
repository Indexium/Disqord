using System;
using System.Collections.Generic;
using Disqord.Models;
using Microsoft.Extensions.Logging;
using Qommon.Collections.ReadOnly;

namespace Disqord.AuditLogs;

public class TransientChannelAuditLogChanges : IChannelAuditLogChanges
{
    /// <inheritdoc/>
    public AuditLogChange<string> Name { get; }

    /// <inheritdoc/>
    public AuditLogChange<string?> Topic { get; }

    /// <inheritdoc/>
    public AuditLogChange<int> Bitrate { get; }

    /// <inheritdoc/>
    public AuditLogChange<int> MemberLimit { get; }

    /// <inheritdoc/>
    public AuditLogChange<IReadOnlyList<IOverwrite>> Overwrites { get; }

    /// <inheritdoc/>
    public AuditLogChange<bool> IsAgeRestricted { get; }

    /// <inheritdoc/>
    public AuditLogChange<TimeSpan> Slowmode { get; }

    /// <inheritdoc/>
    public AuditLogChange<ChannelType> Type { get; }

    /// <inheritdoc/>
    public AuditLogChange<string?> Region { get; }

    /// <inheritdoc/>
    public AuditLogChange<GuildChannelFlags> Flags { get; }

    /// <inheritdoc/>
    public AuditLogChange<VideoQualityMode> VideoQualityMode { get; }

    /// <inheritdoc/>
    public AuditLogChange<TimeSpan> DefaultAutomaticArchiveDuration { get; }

    /// <inheritdoc/>
    public AuditLogChange<TimeSpan> DefaultThreadSlowmode { get; }

    /// <inheritdoc/>
    public AuditLogChange<IReadOnlyList<IForumTag>> AvailableTags { get; }

    /// <inheritdoc/>
    public AuditLogChange<IEmoji?> DefaultReactionEmoji { get; }

    /// <inheritdoc/>
    public AuditLogChange<string?> Template { get; }

    public TransientChannelAuditLogChanges(IClient client, AuditLogEntryJsonModel model)
    {
        for (var i = 0; i < model.Changes.Value.Length; i++)
        {
            var change = model.Changes.Value[i];
            switch (change.Key)
            {
                case "name":
                {
                    Name = AuditLogChange<string>.Convert(change);
                    break;
                }
                case "topic":
                {
                    Topic = AuditLogChange<string?>.Convert(change);
                    break;
                }
                case "bitrate":
                {
                    Bitrate = AuditLogChange<int>.Convert(change);
                    break;
                }
                case "user_limit":
                {
                    MemberLimit = AuditLogChange<int>.Convert(change);
                    break;
                }
                case "permission_overwrites":
                {
                    Overwrites = AuditLogChange<IReadOnlyList<IOverwrite>>.Convert(change, (client, model.TargetId!.Value),
                        (OverwriteJsonModel[] models, (IClient, Snowflake) state) => models.ToReadOnlyList(state, (model, state) =>
                        {
                            var (client, channelId) = state;
                            return new TransientOverwrite(client, channelId, model);
                        }));

                    break;
                }
                case "nsfw":
                {
                    IsAgeRestricted = AuditLogChange<bool>.Convert(change);
                    break;
                }
                case "rate_limit_per_user":
                {
                    Slowmode = AuditLogChange<TimeSpan>.Convert<int>(change, x => TimeSpan.FromSeconds(x));
                    break;
                }
                case "type":
                {
                    Type = AuditLogChange<ChannelType>.Convert(change);
                    break;
                }
                case "rtc_region":
                {
                    Region = AuditLogChange<string?>.Convert(change);
                    break;
                }
                case "flags":
                {
                    Flags = AuditLogChange<GuildChannelFlags>.Convert(change);
                    break;
                }
                case "video_quality_mode":
                {
                    VideoQualityMode = AuditLogChange<VideoQualityMode>.Convert(change);
                    break;
                }
                case "default_auto_archive_duration":
                {
                    DefaultAutomaticArchiveDuration = AuditLogChange<TimeSpan>.Convert<int>(change, x => TimeSpan.FromMinutes(x));
                    break;
                }
                case "default_thread_rate_limit_per_user":
                {
                    DefaultThreadSlowmode = AuditLogChange<TimeSpan>.Convert<int>(change, x => TimeSpan.FromSeconds(x));
                    break;
                }
                case "available_tags":
                {
                    AvailableTags = AuditLogChange<IReadOnlyList<IForumTag>>.Convert<ForumTagJsonModel[]>(change,
                        static models => models.ToReadOnlyList(static model => new TransientForumTag(model)));

                    break;
                }
                case "default_reaction_emoji":
                {
                    DefaultReactionEmoji = AuditLogChange<IEmoji?>.Convert<ForumDefaultReactionJsonModel?>(change, static model =>
                    {
                        if (model == null)
                            return null;

                        if (model.EmojiId != null)
                            return new TransientCustomEmoji(model.EmojiId.Value);

                        return new TransientEmoji(model.EmojiName!);
                    });

                    break;
                }
                case "template":
                {
                    Template = AuditLogChange<string?>.Convert(change);
                    break;
                }
                default:
                {
                    client.Logger.LogDebug("Unknown key {0} for {1}", change.Key, this);
                    break;
                }
            }
        }
    }
}
