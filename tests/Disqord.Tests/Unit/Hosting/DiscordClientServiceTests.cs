using System.Threading;
using System.Threading.Tasks;
using Disqord.Hosting;

namespace Disqord.Tests.Unit.Hosting;

public class DiscordClientServiceTests
{
    [Test]
    public async Task StopAsync_ExecuteTaskRunning_AwaitsExecuteTaskBeforeReturning()
    {
        // Arrange
        var service = new TestService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        Assert.That(service.ExecuteCompleted, Is.True);
    }

    private sealed class TestService : DiscordClientService
    {
        public bool ExecuteCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
            }
            finally
            {
                ExecuteCompleted = true;
            }
        }
    }
}
