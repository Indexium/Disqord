using System;
using System.Collections.Generic;
using Disqord.Models;
using Qommon;

namespace Disqord;

public class TransientMember(IClient client, Snowflake guildId, MemberJsonModel model)
    : TransientUser(client, model.User.Value), IMember, ITransientClientEntity<MemberJsonModel>
{
    /// <inheritdoc/>
    public Snowflake GuildId { get; } = guildId;

    /// <inheritdoc/>
    public string? Nick => Model.Nick.GetValueOrDefault();

    /// <inheritdoc/>
    public IReadOnlyList<Snowflake> RoleIds => Model.Roles;

    /// <inheritdoc/>
    public Optional<DateTimeOffset> JoinedAt => Model.JoinedAt;

    /// <inheritdoc/>
    public bool IsMuted => Model.Mute.GetValueOrDefault();

    /// <inheritdoc/>
    public bool IsDeafened => Model.Deaf.GetValueOrDefault();

    /// <inheritdoc/>
    public DateTimeOffset? BoostedAt => Model.PremiumSince.GetValueOrDefault();

    /// <inheritdoc/>
    public bool IsPending => Model.Pending.GetValueOrDefault();

    /// <inheritdoc/>
    public string? GuildAvatarHash => Model.Avatar.GetValueOrDefault();

    /// <inheritdoc/>
    public DateTimeOffset? TimedOutUntil => Model.CommunicationDisabledUntil.GetValueOrDefault();

    /// <inheritdoc/>
    public MemberFlags GuildFlags => Model.Flags;

    /// <inheritdoc/>
    public IAvatarDecoration? GuildAvatarDecoration
    {
        get
        {
            if (!Model.AvatarDecorationData.HasValue || Model.AvatarDecorationData.Value == null)
                return null;

            return field ??= new TransientAvatarDecoration(Model.AvatarDecorationData.Value);
        }
    }

    /// <inheritdoc/>
    public ICollectibles? GuildCollectibles
    {
        get
        {
            if (!Model.Collectibles.HasValue || Model.Collectibles.Value == null)
                return null;

            return field ??= new TransientCollectibles(Model.Collectibles.Value);
        }
    }

    /// <inheritdoc/>
    public new MemberJsonModel Model { get; } = model;
}
