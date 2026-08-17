using Disqord.Models;
using Qommon;

namespace Disqord;

public class TransientRoleTags(RoleTagsJsonModel model)
    : TransientEntity<RoleTagsJsonModel>(model), IRoleTags
{
    /// <inheritdoc/>
    public Snowflake? BotId => Model.BotId.GetValueOrNullable();

    /// <inheritdoc/>
    public Snowflake? IntegrationId => Model.IntegrationId.GetValueOrNullable();

    /// <inheritdoc/>
    public bool IsNitroBooster => Model.PremiumSubscriber.HasValue;

    /// <inheritdoc/>
    public Snowflake? SubscriptionListingId => Model.SubscriptionListingId.GetValueOrNullable();

    /// <inheritdoc/>
    public bool IsAvailableForPurchase => Model.AvailableForPurchase.HasValue;

    /// <inheritdoc/>
    public bool HasGuildConnections => Model.GuildConnections.HasValue;
}
