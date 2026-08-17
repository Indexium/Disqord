using Disqord.Serialization.Json;
using Qommon;

namespace Disqord.Models;

public class CollectiblesJsonModel : JsonModel
{
    [JsonProperty("nameplate")]
    public Optional<NameplateJsonModel?> Nameplate;
}
