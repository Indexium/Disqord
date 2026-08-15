using Disqord.Models;

namespace Disqord;

public class TransientNameplate(NameplateJsonModel model)
    : TransientEntity<NameplateJsonModel>(model), INameplate
{
    /// <inheritdoc/>
    public Snowflake? SkuId => Model.SkuId;

    /// <inheritdoc/>
    public string Asset => Model.Asset;

    /// <inheritdoc/>
    public string Label => Model.Label;

    /// <inheritdoc/>
    public string Palette => Model.Palette;
}
