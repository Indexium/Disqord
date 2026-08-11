using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Api;
using Disqord.Rest;
using Disqord.Rest.Api;
using Microsoft.Extensions.Logging;

namespace Disqord.Tests.Unit.Rest;

public class PagedEnumeratorTests
{
    [Test]
    public async Task MoveNextAsync_FinalPageExactlyPageSizeWithNoMoreSignal_StopsCleanlyWithoutFetchingAgain()
    {
        // Arrange
        var page = new[] { "a", "b" };
        var enumerator = new TestPagedEnumerator(pageSize: 2, remaining: 2, page: page);

        // Act
        var firstMoveNext = await enumerator.MoveNextAsync();
        var remainingAfterFirstPage = enumerator.RemainingCount;
        var secondMoveNext = await enumerator.MoveNextAsync();

        // Assert
        Assert.That(firstMoveNext, Is.True);
        Assert.That(remainingAfterFirstPage, Is.EqualTo(0));
        Assert.That(secondMoveNext, Is.False);
        Assert.That(enumerator.NextPageCoreAsyncCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task MoveNextAsync_NoSubclassSignal_RecomputesRemainingCountNormally()
    {
        // Arrange
        var pages = new Queue<IReadOnlyList<string>>();
        pages.Enqueue(new[] { "a", "b" });
        pages.Enqueue(new[] { "c" });
        var enumerator = new UnsignaledTestPagedEnumerator(pageSize: 2, remaining: 3, pages: pages);

        // Act
        var firstMoveNext = await enumerator.MoveNextAsync();
        var remainingAfterFirstPage = enumerator.RemainingCount;
        var secondMoveNext = await enumerator.MoveNextAsync();
        var remainingAfterSecondPage = enumerator.RemainingCount;
        var thirdMoveNext = await enumerator.MoveNextAsync();

        // Assert
        Assert.That(firstMoveNext, Is.True);
        Assert.That(remainingAfterFirstPage, Is.EqualTo(1));
        Assert.That(secondMoveNext, Is.True);
        Assert.That(remainingAfterSecondPage, Is.EqualTo(0));
        Assert.That(thirdMoveNext, Is.False);
        Assert.That(enumerator.NextPageCoreAsyncCallCount, Is.EqualTo(2));
    }

    private sealed class TestPagedEnumerator : PagedEnumerator<string>
    {
        public override int PageSize { get; }

        public int NextPageCoreAsyncCallCount { get; private set; }

        private readonly IReadOnlyList<string> _page;

        public TestPagedEnumerator(int pageSize, int remaining, IReadOnlyList<string> page)
            : base(new UnusedRestClient(), remaining)
        {
            PageSize = pageSize;
            _page = page;
        }

        protected override Task<IReadOnlyList<string>> NextPageCoreAsync(IReadOnlyList<string>? previousPage,
            IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            NextPageCoreAsyncCallCount++;

            // Mimics the real archived-threads/pinned-messages enumerators, which set
            // RemainingCount = 0 themselves once the API reports there are no more pages.
            RemainingCount = 0;
            return Task.FromResult(_page);
        }
    }

    private sealed class UnsignaledTestPagedEnumerator : PagedEnumerator<string>
    {
        public override int PageSize { get; }

        public int NextPageCoreAsyncCallCount { get; private set; }

        private readonly Queue<IReadOnlyList<string>> _pages;

        public UnsignaledTestPagedEnumerator(int pageSize, int remaining, Queue<IReadOnlyList<string>> pages)
            : base(new UnusedRestClient(), remaining)
        {
            PageSize = pageSize;
            _pages = pages;
        }

        protected override Task<IReadOnlyList<string>> NextPageCoreAsync(IReadOnlyList<string>? previousPage,
            IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            NextPageCoreAsyncCallCount++;

            // Unlike TestPagedEnumerator, this never signals completion itself, so the base
            // class must keep recomputing RemainingCount from GetConsumedCount as before.
            return Task.FromResult(_pages.Dequeue());
        }
    }

    private sealed class UnusedRestClient : IRestClient
    {
        public IDictionary<Snowflake, IDirectChannel>? DirectChannels => null;

        public IRestApiClient ApiClient => throw new NotSupportedException();

        public ILogger Logger => throw new NotSupportedException();
    }
}
