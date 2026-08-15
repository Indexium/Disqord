using System;
using System.Collections.Generic;
using System.Globalization;
using Disqord.Gateway;
using Disqord.Gateway.Api.Models;
using Disqord.Models;
using Qommon;
using Qommon.Collections.ReadOnly;

namespace Disqord;

public class TransientGatewayGuild(IClient client, GatewayGuildJsonModel model)
    : TransientGatewayClientEntity<GatewayGuildJsonModel>(client, model), IGatewayGuild, ITransientEntity<GuildJsonModel>
{
    public Snowflake Id => Model.Id;

    public string Name => Model.Name;

    public string? IconHash => Model.Icon;

    public string? SplashHash => Model.Splash;

    public string? DiscoverySplashHash => Model.DiscoverySplash.GetValueOrDefault();

    public Snowflake OwnerId => Model.OwnerId;

    public Snowflake? AfkChannelId => Model.AfkChannelId;

    public TimeSpan AfkTimeout => TimeSpan.FromSeconds(Model.AfkTimeout);

    public bool IsWidgetEnabled => Model.WidgetEnabled.GetValueOrDefault();

    public Snowflake? WidgetChannelId => Model.WidgetChannelId.GetValueOrDefault();

    public GuildVerificationLevel VerificationLevel => Model.VerificationLevel;

    public GuildNotificationLevel NotificationLevel => Model.DefaultMessageNotifications;

    public GuildContentFilterLevel ContentFilterLevel => Model.ExplicitContentFilter;

    public IReadOnlyDictionary<Snowflake, IRole> Roles => field ??= Model.Roles.ToReadOnlyDictionary((Client, Id),
        (model, _) => model.Id,
        (model, state) =>
        {
            var (client, guildId) = state;
            return new TransientRole(client, guildId, model) as IRole;
        });

    public IReadOnlyDictionary<Snowflake, IGuildEmoji> Emojis => field ??= Model.Emojis.ToReadOnlyDictionary((Client, Id),
        (model, _) => model.Id!.Value,
        (model, state) =>
        {
            var (client, guildId) = state;
            return new TransientGuildEmoji(client, guildId, model) as IGuildEmoji;
        });

    public IReadOnlyList<string> Features => Model.Features;

    public GuildMfaLevel MfaLevel => Model.MfaLevel;

    public Snowflake? ApplicationId => Model.ApplicationId;

    public Snowflake? SystemChannelId => Model.SystemChannelId;

    public SystemChannelFlags SystemChannelFlags => Model.SystemChannelFlags;

    public Snowflake? RulesChannelId => Model.RulesChannelId;

    public int? MaxPresenceCount => Model.MaxPresences.GetValueOrDefault();

    public int? MaxMemberCount => Model.MaxMembers.GetValueOrNullable();

    public string? VanityUrlCode => Model.VanityUrlCode;

    public string? Description => Model.Description;

    public string? BannerHash => Model.Banner;

    public GuildBoostTier BoostTier => Model.PremiumTier;

    public int? BoostingMemberCount => Model.PremiumSubscriptionCount.GetValueOrNullable();

    public CultureInfo PreferredLocale => Discord.Internal.GetLocale(Model.PreferredLocale);

    public Snowflake? PublicUpdatesChannelId => Model.PublicUpdatesChannelId;

    public int? MaxVideoMemberCount => Model.MaxVideoChannelUsers.GetValueOrNullable();

    public int? MaxStageVideoMemberCount => Model.MaxStageVideoChannelUsers.GetValueOrNullable();

    public GuildNsfwLevel NsfwLevel => Model.NsfwLevel;

    public IReadOnlyDictionary<Snowflake, IGuildSticker> Stickers
    {
        get
        {
            if (!Model.Stickers.HasValue)
                return ReadOnlyDictionary<Snowflake, IGuildSticker>.Empty;

            return field ??= Model.Stickers.Value.ToReadOnlyDictionary(Client,
                (model, _) => model.Id,
                (model, client) => new TransientGuildSticker(client, model) as IGuildSticker);
        }
    }

    public bool IsBoostProgressBarEnabled => Model.PremiumProgressBarEnabled;

    public Snowflake? SafetyAlertsChannelId => Model.SafetyAlertsChannelId;

    public IGuildIncidents? Incidents
    {
        get
        {
            if (!Model.IncidentsData.HasValue || Model.IncidentsData.Value == null)
                return null;

            return field ??= new TransientGuildIncidents(Model.IncidentsData.Value);
        }
    }

    public DateTimeOffset JoinedAt => Model.JoinedAt;

    public bool IsLarge => Model.Large;

    public bool IsUnavailable => Model.Unavailable.GetValueOrDefault();

    public int MemberCount => Model.MemberCount;

    public IReadOnlyDictionary<Snowflake, IVoiceState> VoiceStates => field ??= Model.VoiceStates.SafelyDeserializeItems<VoiceStateJsonModel>(Client.Logger).ToReadOnlyDictionary(Client,
        (model, _) => model.UserId,
        (model, client) => new TransientVoiceState(client, model) as IVoiceState);

    public IReadOnlyDictionary<Snowflake, IMember> Members => field ??= Model.Members.SafelyDeserializeItems<MemberJsonModel>(Client.Logger).ToReadOnlyDictionary((Client, Id),
        (model, _) => model.User.Value.Id, (model, state) =>
        {
            var (client, guildId) = state;
            return new TransientMember(client, guildId, model) as IMember;
        });

    public IReadOnlyDictionary<Snowflake, IGuildChannel> Channels => field ??= Model.Channels.SafelyDeserializeItems<ChannelJsonModel>(Client.Logger).ToReadOnlyDictionary(Client,
        (model, _) => model.Id,
        (model, client) => TransientGuildChannel.Create(client, model) as IGuildChannel);

    public IReadOnlyDictionary<Snowflake, IPresence> Presences => field ??= Model.Presences.SafelyDeserializeItems<PresenceJsonModel>(Client.Logger).ToReadOnlyDictionary(Client,
        (model, _) => model.User.Id,
        (model, client) => new TransientPresence(client, model) as IPresence);

    public IReadOnlyDictionary<Snowflake, IStage> Stages => field ??= Model.StageInstances.SafelyDeserializeItems<StageInstanceJsonModel>(Client.Logger).ToReadOnlyDictionary(Client,
        (model, _) => model.Id,
        (model, client) => new TransientStage(client, model) as IStage);

    public IReadOnlyDictionary<Snowflake, IGuildEvent> GuildEvents => field ??= Model.GuildScheduledEvents.SafelyDeserializeItems<GuildScheduledEventJsonModel>(Client.Logger).ToReadOnlyDictionary(Client,
        (model, _) => model.Id,
        (model, client) => new TransientGuildEvent(client, model) as IGuildEvent);

    GuildJsonModel ITransientEntity<GuildJsonModel>.Model => Model;

    public override string ToString()
    {
        return this.GetString();
    }
}
