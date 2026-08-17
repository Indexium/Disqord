namespace Disqord;

/// <summary>
///     Represents the collectibles of a user.
/// </summary>
public interface ICollectibles
{
    /// <summary>
    ///     Gets the nameplate collectible of this user.
    /// </summary>
    /// <returns>
    ///     The nameplate or <see langword="null"/> if the user has no nameplate.
    /// </returns>
    INameplate? Nameplate { get; }
}
