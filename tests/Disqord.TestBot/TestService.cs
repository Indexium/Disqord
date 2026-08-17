using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;

namespace Disqord.TestBot
{
    public class TestService : DiscordBotService
    {
        // Fired whenever a shard handles the READY dispatch.
        protected override ValueTask OnReady(ReadyEventArgs e)
        {
            Logger.LogInformation("Ready fired for {ShardId}!", e.ShardId);
            return default;
        }

        // Long-running logic goes here...
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);
            Logger.LogInformation("Client is ready.");

            var isBeep = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var activity = new LocalActivity(!isBeep ? "Beep" : "Boop", ActivityType.Playing);
                    isBeep = !isBeep;
                    await Bot.SetPresenceAsync(activity, stoppingToken);

                    await Task.Delay(12_500, stoppingToken);
                }
                catch
                {
                    await Task.Delay(30_000, stoppingToken);
                }
            }
        }

        // Fired if the user doesn't provide a prefix or it's a system message etc.
        protected override ValueTask OnNonCommandReceived(BotMessageReceivedEventArgs e)
        {
            Logger.LogInformation("Received a non command: {0}", e.Message.Content);
            return default;
        }

        // Fired when an attempt is made to execute a command but it's not a valid one.
        protected override ValueTask OnCommandNotFound(IDiscordCommandContext context)
        {
            if (context is IDiscordTextCommandContext textContext)
                Logger.LogInformation("Text command not found: {0}", textContext.Message.Content);

            return default;
        }
    }
}
