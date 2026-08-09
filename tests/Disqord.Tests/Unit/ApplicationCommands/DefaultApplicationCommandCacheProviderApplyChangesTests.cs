using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Application.Default;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Disqord.Tests.Unit.ApplicationCommands;

public class DefaultApplicationCommandCacheProviderApplyChangesTests
{
    private static DefaultApplicationCommandCacheProvider CreateProvider(string directoryPath)
    {
        var configuration = new DefaultApplicationCommandCacheProviderConfiguration
        {
            DirectoryPath = directoryPath
        };

        return new DefaultApplicationCommandCacheProvider(
            Options.Create(configuration),
            NullLogger<DefaultApplicationCommandCacheProvider>.Instance,
            new Disqord.Serialization.Json.Default.DefaultJsonSerializer());
    }

    [Test]
    public async Task ApplyChanges_UnchangedCommandRefreshedWithNewIdFromBulkResync_UpdatesCachedId()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-cache-test-{Guid.NewGuid():N}");
        var provider = CreateProvider(directoryPath);

        try
        {
            var command = new LocalSlashCommand
            {
                Name = "test",
                Description = "A test command.",
            };

            var cache = (DefaultApplicationCommandCacheProvider.Cache) await provider.GetCacheAsync(CancellationToken.None);

            var initialChanges = cache.GetChanges(null, new LocalApplicationCommand[] { command });
            cache.ApplyChanges(null, initialChanges, new IApplicationCommand[]
            {
                new FakeApplicationCommand(100, "test", ApplicationCommandType.Slash),
            });

            Assert.That(cache.Model.GlobalCommands![0].Id, Is.EqualTo((Snowflake) 100));

            var secondChanges = cache.GetChanges(null, new LocalApplicationCommand[] { command });
            Assert.That(secondChanges.UnchangedCommands, Does.Contain(command));

            // Act
            cache.ApplyChanges(null, secondChanges, new IApplicationCommand[]
            {
                new FakeApplicationCommand(200, "test", ApplicationCommandType.Slash),
            });

            // Assert
            Assert.That(cache.Model.GlobalCommands![0].Id, Is.EqualTo((Snowflake) 200));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public async Task ApplyChanges_SlashAndUserCommandsShareName_DoesNotThrow()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-cache-test-{Guid.NewGuid():N}");
        var provider = CreateProvider(directoryPath);

        try
        {
            var slashCommand = new LocalSlashCommand
            {
                Name = "profile",
                Description = "Shows a profile.",
            };

            var userCommand = new LocalUserContextMenuCommand
            {
                Name = "profile",
            };

            var cache = (DefaultApplicationCommandCacheProvider.Cache) await provider.GetCacheAsync(CancellationToken.None);
            var changes = cache.GetChanges(null, new LocalApplicationCommand[] { slashCommand, userCommand });

            // Act & Assert
            Assert.That(() => cache.ApplyChanges(null, changes, new IApplicationCommand[]
            {
                new FakeApplicationCommand(100, "profile", ApplicationCommandType.Slash),
                new FakeApplicationCommand(101, "profile", ApplicationCommandType.User),
            }), Throws.Nothing);

            Assert.That(cache.Model.GlobalCommands, Has.Length.EqualTo(2));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
