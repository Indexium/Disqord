using System.Collections.Generic;
using Disqord.Models;

namespace Disqord;

public class TransientBulkBanResponse(GuildBulkBanJsonModel model)
    : TransientEntity<GuildBulkBanJsonModel>(model), IBulkBanResponse
{
    /// <inheritdoc/>
    public IReadOnlyList<Snowflake> BannedUserIds => Model.BannedUsers;

    /// <inheritdoc/>
    public IReadOnlyList<Snowflake> FailedUserIds => Model.FailedUsers;
}
