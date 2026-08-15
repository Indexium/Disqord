using Disqord.Models;

namespace Disqord;

public class TransientAvatarDecoration(AvatarDecorationDataJsonModel model)
    : TransientEntity<AvatarDecorationDataJsonModel>(model), IAvatarDecoration
{
    /// <inheritdoc/>
    public string AssetHash => Model.Asset;

    /// <inheritdoc/>
    public Snowflake? SkuId => Model.SkuId;
}
