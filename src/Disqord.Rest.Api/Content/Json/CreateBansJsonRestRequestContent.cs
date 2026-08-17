using System.Collections.Generic;
using Disqord.Serialization.Json;
using Qommon;

namespace Disqord.Rest.Api;

public class CreateBansJsonRestRequestContent : JsonModelRestRequestContent
{
    [JsonProperty("user_ids")]
    public IList<Snowflake> UserIds = null!;

    [JsonProperty("delete_message_seconds")]
    public Optional<int> DeleteMessageSeconds;

    protected override void OnValidate()
    {
        Guard.HasSizeLessThanOrEqualTo(UserIds, Discord.Limits.Rest.BulkBanUsersPageSize);
        OptionalGuard.CheckValue(DeleteMessageSeconds, static seconds => Guard.IsBetweenOrEqualTo(seconds, 0, Discord.Limits.Rest.BanDeleteMessageSeconds));
    }
}
