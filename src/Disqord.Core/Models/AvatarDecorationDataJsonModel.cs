using Disqord.Serialization.Json;

namespace Disqord.Models;

public class AvatarDecorationDataJsonModel : JsonModel
{
    [JsonProperty("asset")]
    public string Asset = null!;

    [JsonProperty("sku_id")]
    public Snowflake? SkuId;
}
