using Disqord.Models;
using Disqord.Serialization.Json;

namespace Disqord.Gateway.Api.Models;

public class ThreadMemberUpdateJsonModel : ThreadMemberJsonModel
{
    [JsonProperty("guild_id")]
    public Snowflake GuildId;
}
