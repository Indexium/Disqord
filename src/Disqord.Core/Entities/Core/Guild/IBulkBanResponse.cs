using System.Collections.Generic;
using Disqord.Models;

namespace Disqord;

/// <summary>
///     Represents the result of a bulk guild ban.
/// </summary>
public interface IBulkBanResponse : IEntity, IJsonUpdatable<GuildBulkBanJsonModel>
{
    /// <summary>
    ///     Gets the IDs of the users that were successfully banned.
    /// </summary>
    IReadOnlyList<Snowflake> BannedUserIds { get; }

    /// <summary>
    ///     Gets the IDs of the users that could not be banned.
    /// </summary>
    IReadOnlyList<Snowflake> FailedUserIds { get; }
}
