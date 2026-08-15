using Disqord.Models;
using Qommon;

namespace Disqord.Rest;

public class TransientRestMember(IClient client, Snowflake guildId, MemberJsonModel model)
    : TransientMember(client, guildId, model), IRestMember
{
    /// <inheritdoc/>
    public string? GuildBannerHash => Model.Banner.GetValueOrDefault();
}
