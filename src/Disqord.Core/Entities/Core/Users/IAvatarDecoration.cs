namespace Disqord;

/// <summary>
///     Represents the avatar decoration of a user.
/// </summary>
public interface IAvatarDecoration
{
    /// <summary>
    ///     Gets the image hash of this avatar decoration.
    /// </summary>
    string AssetHash { get; }

    /// <summary>
    ///     Gets the ID of the SKU this avatar decoration belongs to.
    /// </summary>
    /// <returns>
    ///     The ID of the SKU or <see langword="null"/> if not set.
    /// </returns>
    Snowflake? SkuId { get; }
}
