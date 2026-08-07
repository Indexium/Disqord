using Disqord.Bot.Commands.Application.Default;
using Disqord.Models;
using Disqord.Serialization.Json;
using Disqord.Tests.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Disqord.Tests.Unit.ApplicationCommands;

public class DefaultApplicationCommandCacheProviderTests : SerializationTestBase
{
    [Test]
    public async Task DisposeCacheAsync_CacheFileDoesNotExist_CreatesFileWithoutLeavingTemporaryFileBehind()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-cache-test-{Guid.NewGuid():N}");
        var configuration = new DefaultApplicationCommandCacheProviderConfiguration
        {
            DirectoryPath = directoryPath
        };

        var provider = new DefaultApplicationCommandCacheProvider(
            Options.Create(configuration),
            NullLogger<DefaultApplicationCommandCacheProvider>.Instance,
            Serializer);

        try
        {
            // Act
            var cache = await provider.GetCacheAsync(CancellationToken.None);
            await cache.DisposeAsync();

            // Assert
            Assert.That(File.Exists(provider.FilePath), Is.True);
            Assert.That(File.Exists(provider.TemporaryFilePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public async Task DisposeCacheAsync_CacheFileDoesNotExistAndTemporaryFileIsBlocked_DoesNotWriteDirectlyToTheRealFile()
    {
        // Arrange
        var directoryPath = Path.Combine(Path.GetTempPath(), $"disqord-cache-test-{Guid.NewGuid():N}");
        var configuration = new DefaultApplicationCommandCacheProviderConfiguration
        {
            DirectoryPath = directoryPath
        };

        var provider = new DefaultApplicationCommandCacheProvider(
            Options.Create(configuration),
            NullLogger<DefaultApplicationCommandCacheProvider>.Instance,
            Serializer);

        try
        {
            var cache = await provider.GetCacheAsync(CancellationToken.None);

            Directory.CreateDirectory(provider.TemporaryFilePath);

            // Act & Assert
            Assert.That(async () => await cache.DisposeAsync(), Throws.InstanceOf<InvalidOperationException>());
            Assert.That(File.Exists(provider.FilePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public void NumericValue_RoundTrippedAndCompared_AreEqual()
    {
        // Arrange
        var choice = new LocalSlashCommandOptionChoice
        {
            Name = "Foo",
            Value = 42,
        };

        // Act
        var equal = RoundTripChoice(choice).Equals(choice);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void StringValue_RoundTrippedAndCompared_AreEqual()
    {
        // Arrange
        var choice = new LocalSlashCommandOptionChoice
        {
            Name = "Bar",
            Value = "Hello",
        };

        // Act
        var equal = RoundTripChoice(choice).Equals(choice);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void DoubleValue_RoundTrippedAndCompared_AreEqual()
    {
        // Arrange
        var choice = new LocalSlashCommandOptionChoice
        {
            Name = "Baz",
            Value = 3.14,
        };

        // Act
        var equal = RoundTripChoice(choice).Equals(choice);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void OptionFileTypes_ReorderedAndCompared_AreEqual()
    {
        // Arrange
        var option = new LocalSlashCommandOption
        {
            Type = SlashCommandOptionType.Attachment,
            Name = "file",
            Description = "A file",
        }.WithFileTypes("image", ".pdf");

        var model = new DefaultApplicationCommandCacheProvider.OptionJsonModel(option, Serializer);
        var reorderedOption = new LocalSlashCommandOption
        {
            Type = SlashCommandOptionType.Attachment,
            Name = "file",
            Description = "A file",
        }.WithFileTypes(".pdf", "image");

        // Act
        var equal = model.Equals(reorderedOption);

        // Assert
        Assert.That(equal, Is.True);
    }

    [Test]
    public void OptionFileTypes_Changed_AreNotEqual()
    {
        // Arrange
        var option = new LocalSlashCommandOption
        {
            Type = SlashCommandOptionType.Attachment,
            Name = "file",
            Description = "A file",
        }.WithFileTypes("image");

        var model = new DefaultApplicationCommandCacheProvider.OptionJsonModel(option, Serializer);
        var changedOption = new LocalSlashCommandOption
        {
            Type = SlashCommandOptionType.Attachment,
            Name = "file",
            Description = "A file",
        }.WithFileTypes("video");

        // Act
        var equal = model.Equals(changedOption);

        // Assert
        Assert.That(equal, Is.False);
    }

    private DefaultApplicationCommandCacheProvider.ChoiceJsonModel RoundTripChoice(LocalSlashCommandOptionChoice choice)
    {
        var model = new DefaultApplicationCommandCacheProvider.ChoiceJsonModel(choice, Serializer);
        var json = Serialize(model);
        var deserialized = Deserialize<DefaultApplicationCommandCacheProvider.ChoiceJsonModel>(json);
        deserialized.SetSerializer(Serializer);
        return deserialized;
    }

    [Test]
    public void DefaultJsonNode_ToTypeObject_ReturnsCLRPrimitive()
    {
        // Arrange
        var jsonValue = Serializer.GetJsonNode(42)!;

        // Act
        var result = jsonValue.ToType<object>();

        // Assert
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void OnValidate_WithIntegerValue_DoesNotThrow()
    {
        // Arrange
        var model = new ApplicationCommandOptionChoiceJsonModel
        {
            Name = "Test",
            Value = (Serializer.GetJsonNode(42) as IJsonValue)!,
        };

        // Act & Assert
        Assert.That(() => model.Validate(), Throws.Nothing);
    }

    [Test]
    public void OnValidate_WithStringValue_DoesNotThrow()
    {
        // Arrange
        var model = new ApplicationCommandOptionChoiceJsonModel
        {
            Name = "Test",
            Value = (Serializer.GetJsonNode("hello") as IJsonValue)!,
        };

        // Act & Assert
        Assert.That(() => model.Validate(), Throws.Nothing);
    }

    [Test]
    public void OnValidate_WithDoubleValue_DoesNotThrow()
    {
        // Arrange
        var model = new ApplicationCommandOptionChoiceJsonModel
        {
            Name = "Test",
            Value = (Serializer.GetJsonNode(3.14) as IJsonValue)!,
        };

        // Act & Assert
        Assert.That(() => model.Validate(), Throws.Nothing);
    }

    [Test]
    public void OnValidate_WithLongValue_DoesNotThrow()
    {
        // Arrange
        var model = new ApplicationCommandOptionChoiceJsonModel
        {
            Name = "Test",
            Value = (Serializer.GetJsonNode(1234567890123L) as IJsonValue)!,
        };

        // Act & Assert
        Assert.That(() => model.Validate(), Throws.Nothing);
    }
}
