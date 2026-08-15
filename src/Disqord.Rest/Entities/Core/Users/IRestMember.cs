namespace Disqord.Rest;

/// <inheritdoc cref="IMember"/>
public interface IRestMember : IMember
{
    /// <summary>
    ///     Gets the guild banner image hash of this member.
    ///     Returns <see langword="null"/> if this member has no guild banner set.
    /// </summary>
    string? GuildBannerHash { get; }
}
