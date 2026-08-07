using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;

namespace Disqord.Tests.Unit.Interactivity;

public class MenuBaseInteractionConcurrencyTests
{
    [Test]
    public async Task OnInteractionReceived_CalledConcurrently_SerializesHandleInteractionAsyncCalls()
    {
        // Arrange
        var menu = new TestMenu(new TestView());
        var args = new InteractionReceivedEventArgs(null!, null);

        // Act
        await Task.WhenAll(
            menu.OnInteractionReceived(args).AsTask(),
            menu.OnInteractionReceived(args).AsTask());

        // Assert
        Assert.That(menu.MaxObservedConcurrency, Is.EqualTo(1));
    }

    private sealed class TestView : ViewBase
    {
        public TestView()
            : base(null)
        { }
    }

    private sealed class TestMenu : MenuBase
    {
        public int MaxObservedConcurrency { get; private set; }

        private int _concurrentCount;
        private readonly object _countLock = new();

        public TestMenu(ViewBase view)
            : base(view)
        { }

        public override LocalMessageBase CreateLocalMessage()
        {
            throw new NotImplementedException();
        }

        protected internal override ValueTask<Snowflake> InitializeAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override async ValueTask HandleInteractionAsync(InteractionReceivedEventArgs e)
        {
            lock (_countLock)
            {
                _concurrentCount++;
                if (_concurrentCount > MaxObservedConcurrency)
                    MaxObservedConcurrency = _concurrentCount;
            }

            await Task.Delay(50).ConfigureAwait(false);

            lock (_countLock)
            {
                _concurrentCount--;
            }
        }
    }
}
