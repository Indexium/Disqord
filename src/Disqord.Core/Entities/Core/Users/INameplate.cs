namespace Disqord;

/// <summary>
///     Represents the nameplate collectible of a user.
/// </summary>
public interface INameplate
{
    /// <summary>
    ///     Gets the ID of the SKU this nameplate belongs to.
    /// </summary>
    /// <returns>
    ///     The ID of the SKU or <see langword="null"/> if not set.
    /// </returns>
    Snowflake? SkuId { get; }

    /// <summary>
    ///     Gets the path to the asset of this nameplate.
    /// </summary>
    string Asset { get; }

    /// <summary>
    ///     Gets the label of this nameplate.
    /// </summary>
    string Label { get; }

    /// <summary>
    ///     Gets the background color palette of this nameplate.
    /// </summary>
    string Palette { get; }
}
