using System.Collections.Generic;
using System.Globalization;
using Disqord.Bot.Commands.Application.Default;
using Disqord.Serialization.Json.Default;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Disqord.Tests.Unit.ApplicationCommands;

public class DefaultApplicationCommandLocalizerStoreTests
{
    private static readonly CultureInfo GermanLocale = CultureInfo.GetCultureInfo("de");

    private static LocalSlashCommand CreateCommandWithNewOptionBeforeExistingOption()
    {
        return new LocalSlashCommand
        {
            Name = "mycommand",
            Description = "My command.",
            Options = new List<LocalSlashCommandOption>
            {
                new()
                {
                    Type = SlashCommandOptionType.String,
                    Name = "newoption",
                    Description = "A brand new option not yet in the localization store.",
                },
                new()
                {
                    Type = SlashCommandOptionType.String,
                    Name = "existingoption",
                    Description = "An option that already has a stored translation.",
                },
            },
        };
    }

    [Test]
    public async Task LocalizeAsync_NewOptionPrecedesExistingOptionInDeclarationOrder_DoesNotDropExistingOptionTranslation()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-localizer-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);

        try
        {
            var serializer = new DefaultJsonSerializer();

            var existingModel = new DefaultApplicationCommandLocalizer.LocalizationStoreJsonModel
            {
                SchemaVersion = DefaultApplicationCommandLocalizer.SchemaVersion,
                GlobalLocalizations = new DefaultApplicationCommandLocalizer.LocalizationNodeJsonModel
                {
                    SlashCommands = new Dictionary<string, DefaultApplicationCommandLocalizer.CommandLocalizationJsonModel?>
                    {
                        ["mycommand"] = new DefaultApplicationCommandLocalizer.CommandLocalizationJsonModel
                        {
                            Name = "meinbefehl",
                            Description = "Mein Befehl.",
                            Options = new Dictionary<string, DefaultApplicationCommandLocalizer.OptionLocalizationJsonModel?>
                            {
                                ["existingoption"] = new DefaultApplicationCommandLocalizer.OptionLocalizationJsonModel
                                {
                                    Name = "vorhandeneoption",
                                    Description = "Eine Option mit einer vorhandenen Uebersetzung.",
                                },
                            },
                        },
                    },
                },
            };

            using (var stream = new FileStream(Path.Combine(directoryPath, "de.json"), FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, existingModel);
            }

            var configuration = new DefaultApplicationCommandLocalizerConfiguration
            {
                DirectoryPath = directoryPath,
                DefaultCulture = CultureInfo.GetCultureInfo("en-US"),
            };

            var localizer = new DefaultApplicationCommandLocalizer(
                Options.Create(configuration),
                NullLogger<DefaultApplicationCommandLocalizer>.Instance,
                serializer);

            var command = CreateCommandWithNewOptionBeforeExistingOption();

            // Act
            await localizer.LocalizeAsync(new[] { command }, new Dictionary<Snowflake, IEnumerable<LocalApplicationCommand>>());

            // Assert
            var existingOption = ((LocalSlashCommand) command).Options.Value![1];
            Assert.That(existingOption.Name.Value, Is.EqualTo("existingoption"));
            Assert.That(existingOption.NameLocalizations.HasValue, Is.True);
            Assert.That(existingOption.NameLocalizations.Value!.ContainsKey(GermanLocale), Is.True);
            Assert.That(existingOption.NameLocalizations.Value![GermanLocale], Is.EqualTo("vorhandeneoption"));
            Assert.That(existingOption.DescriptionLocalizations.Value![GermanLocale], Is.EqualTo("Eine Option mit einer vorhandenen Uebersetzung."));

            using var readStream = new FileStream(Path.Combine(directoryPath, "de.json"), FileMode.Open, FileAccess.Read);
            var writtenModel = serializer.Deserialize<DefaultApplicationCommandLocalizer.LocalizationStoreJsonModel>(readStream)!;
            var writtenExistingOption = writtenModel.GlobalLocalizations!.SlashCommands.Value!["mycommand"]!.Options.Value!["existingoption"]!;
            Assert.That(writtenExistingOption.Name, Is.EqualTo("vorhandeneoption"));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public async Task LocalizeAsync_DefaultLocaleDescriptionChangedSinceLastRun_DoesNotReapplyStaleDescription()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-localizer-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);

        try
        {
            var serializer = new DefaultJsonSerializer();

            var existingModel = new DefaultApplicationCommandLocalizer.LocalizationStoreJsonModel
            {
                SchemaVersion = DefaultApplicationCommandLocalizer.SchemaVersion,
                GlobalLocalizations = new DefaultApplicationCommandLocalizer.LocalizationNodeJsonModel
                {
                    SlashCommands = new Dictionary<string, DefaultApplicationCommandLocalizer.CommandLocalizationJsonModel?>
                    {
                        ["mycommand"] = new DefaultApplicationCommandLocalizer.CommandLocalizationJsonModel
                        {
                            Name = "mycommand",
                            Description = "The old description.",
                        },
                    },
                },
            };

            using (var stream = new FileStream(Path.Combine(directoryPath, "en-US.json"), FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, existingModel);
            }

            var configuration = new DefaultApplicationCommandLocalizerConfiguration
            {
                DirectoryPath = directoryPath,
                DefaultCulture = CultureInfo.GetCultureInfo("en-US"),
            };

            var localizer = new DefaultApplicationCommandLocalizer(
                Options.Create(configuration),
                NullLogger<DefaultApplicationCommandLocalizer>.Instance,
                serializer);

            var command = new LocalSlashCommand
            {
                Name = "mycommand",
                Description = "The new description.",
            };

            // Act
            await localizer.LocalizeAsync(new LocalApplicationCommand[] { command }, new Dictionary<Snowflake, IEnumerable<LocalApplicationCommand>>());

            // Assert
            Assert.That(command.Description.Value, Is.EqualTo("The new description."));
            Assert.That(command.DescriptionLocalizations.HasValue, Is.False);

            using var readStream = new FileStream(Path.Combine(directoryPath, "en-US.json"), FileMode.Open, FileAccess.Read);
            var writtenModel = serializer.Deserialize<DefaultApplicationCommandLocalizer.LocalizationStoreJsonModel>(readStream)!;
            var writtenCommand = writtenModel.GlobalLocalizations!.SlashCommands.Value!["mycommand"]!;
            Assert.That(writtenCommand.Description.Value, Is.EqualTo("The new description."));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
