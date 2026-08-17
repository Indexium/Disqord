using Disqord.Serialization.Json;

namespace Disqord.Models;

public class NameplateJsonModel : JsonModel
{
    [JsonProperty("sku_id")]
    public Snowflake? SkuId;

    [JsonProperty("asset")]
    public string Asset = null!;

    [JsonProperty("label")]
    public string Label = null!;

    [JsonProperty("palette")]
    public string Palette = null!;
}
