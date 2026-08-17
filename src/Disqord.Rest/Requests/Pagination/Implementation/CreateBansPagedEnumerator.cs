using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Models;
using Qommon;

namespace Disqord.Rest;

public class CreateBansPagedEnumerator(
    IRestClient client,
    Snowflake guildId,
    Snowflake[] userIds,
    Optional<int> deleteMessageSeconds,
    IRestRequestOptions? options,
    CancellationToken cancellationToken)
    : PagedEnumerator<GuildBulkBanJsonModel, IBulkBanResponse>(client, userIds.Length, options, cancellationToken)
{
    public override int PageSize => Discord.Limits.Rest.BulkBanUsersPageSize;

    private int _offset;
    private int _lastConsumedCount;

    protected override async Task<GuildBulkBanJsonModel?> NextPageAsync(
        GuildBulkBanJsonModel? previousPage, IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var amount = NextPageSize;
        var segment = new ArraySegment<Snowflake>(userIds, _offset, amount);
        _offset += amount;
        _lastConsumedCount = amount;

        return await Client.InternalCreateBansAsync(guildId, segment, deleteMessageSeconds, options, cancellationToken).ConfigureAwait(false);
    }

    protected override IReadOnlyList<IBulkBanResponse> GetPageItems(GuildBulkBanJsonModel page)
    {
        return [new TransientBulkBanResponse(page)];
    }

    protected override int GetConsumedCount(GuildBulkBanJsonModel page)
    {
        return _lastConsumedCount;
    }
}
