using System.Reflection;
using Disqord.Gateway;
using Disqord.Gateway.Api.Default;
using Disqord.Hosting;
using Disqord.Rest;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Disqord.Tests.Integration.Gateway;

[TestFixture]
[Category("Integration")]
[DiscordIntegrationTest]
[NonParallelizable]
public class ObfuscatedChannelIntegrationTests : IntegrationTestBase
{
    private const string SecondaryBotTokenVariableName = "DISQORD_INTEGRATION_BOT2_TOKEN";
    private const string ObfuscatedName = "___hidden___";

    private static readonly Snowflake HiddenCategoryId = 1537921509107171379;
    private static readonly Snowflake HiddenTextChannelId = 1537921511132762214;
    private static readonly Snowflake HiddenVoiceChannelId = 1537921512982450400;
    private static readonly Snowflake VisibleTextChannelId = 1537921515457347665;

    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(1);

    private DiscordClientBase SecondaryClient
    {
        get
        {
            Assume.That(_secondaryClient, Is.Not.Null);
            return _secondaryClient!;
        }
    }

    private IHost? _secondaryHost;
    private DiscordClientBase? _secondaryClient;

    [OneTimeSetUp]
    public async Task SetUpSecondaryBot()
    {
        var secondaryBotToken = Environment.GetEnvironmentVariable(SecondaryBotTokenVariableName);
        if (string.IsNullOrWhiteSpace(secondaryBotToken))
        {
            Assert.Ignore($"{SecondaryBotTokenVariableName} not set; skipping integration tests.");
        }

        using var cancellationTokenSource = new CancellationTokenSource(StartupTimeout);

        _secondaryHost = CreateSecondaryHost(secondaryBotToken);
        await _secondaryHost.StartAsync(cancellationTokenSource.Token);

        _secondaryClient = _secondaryHost.Services.GetRequiredService<DiscordClientBase>();
        await _secondaryClient.WaitUntilReadyAsync(cancellationTokenSource.Token);
    }

    [OneTimeTearDown]
    public async Task TearDownSecondaryBot()
    {
        if (_secondaryHost == null)
        {
            return;
        }

        using (var cancellationTokenSource = new CancellationTokenSource(StartupTimeout))
        {
            await _secondaryHost.StopAsync(cancellationTokenSource.Token);
        }

        _secondaryHost.Dispose();
    }

    [TearDown]
    public async Task RestoreObfuscation()
    {
        var channel = _secondaryClient?.GetChannel(IntegrationGuildId, HiddenTextChannelId);
        if (channel == null || channel.IsObfuscated)
        {
            return;
        }

        using var cancellationTokenSource = new CancellationTokenSource(EventTimeout);
        var cancellationToken = cancellationTokenSource.Token;
        var reobfuscatedChannel = WaitForChannelUpdateAsync(HiddenTextChannelId, static updatedChannel => updatedChannel.IsObfuscated, cancellationToken);
        await RevokeSecondaryBotAccessAsync(cancellationToken);
        await reobfuscatedChannel;
    }

    [Test]
    [Order(1)]
    public void ObfuscatedChannel_BotCannotViewTextChannel_IsFlaggedAsObfuscated()
    {
        // Act
        var channel = GetSecondaryChannel(HiddenTextChannelId);

        // Assert
        Assert.That(channel.Flags.HasFlag(GuildChannelFlags.Obfuscated), Is.True);
        Assert.That(channel.IsObfuscated, Is.True);
        Assert.That(channel.Name, Is.EqualTo(ObfuscatedName));
    }

    [Test]
    [Order(2)]
    public void ObfuscatedChannel_BotCannotViewCategoryChannel_IsFlaggedAsObfuscated()
    {
        // Act
        var channel = GetSecondaryChannel(HiddenCategoryId);

        // Assert
        Assert.That(channel, Is.InstanceOf<ICategoryChannel>());
        Assert.That(channel.IsObfuscated, Is.True);
        Assert.That(channel.Name, Is.EqualTo(ObfuscatedName));
    }

    [Test]
    [Order(3)]
    public void ObfuscatedChannel_BotCannotViewVoiceChannel_IsFlaggedAsObfuscated()
    {
        // Act
        var channel = GetSecondaryChannel(HiddenVoiceChannelId);

        // Assert
        Assert.That(channel, Is.InstanceOf<IVoiceChannel>());
        Assert.That(channel.IsObfuscated, Is.True);
        Assert.That(channel.Name, Is.EqualTo(ObfuscatedName));
    }

    [Test]
    [Order(4)]
    public async Task ObfuscatedChannel_BotCanViewChannel_IsNotObfuscated()
    {
        // Arrange
        var expectedChannel = await FetchGuildChannelAsync(VisibleTextChannelId);

        // Act
        var channel = GetSecondaryChannel(VisibleTextChannelId);

        // Assert
        Assert.That(channel.IsObfuscated, Is.False);
        Assert.That(channel.Name, Is.EqualTo(expectedChannel.Name));
    }

    [Test]
    [Order(5)]
    public async Task ObfuscatedChannel_Obfuscated_RetainsIdentifyingMetadata()
    {
        // Arrange
        var expectedChannel = (ICategorizableGuildChannel) await FetchGuildChannelAsync(HiddenTextChannelId);

        // Act
        var channel = (ICategorizableGuildChannel) GetSecondaryChannel(HiddenTextChannelId);

        // Assert
        Assert.That(channel.Id, Is.EqualTo(expectedChannel.Id));
        Assert.That(channel.Type, Is.EqualTo(expectedChannel.Type));
        Assert.That(channel.Position, Is.EqualTo(expectedChannel.Position));
        Assert.That(channel.CategoryId, Is.EqualTo(HiddenCategoryId));
    }

    [Test]
    [Order(6)]
    public void ObfuscatedChannel_Obfuscated_OnlyDeniesViewChannelsForEveryone()
    {
        // Act
        var overwrites = GetSecondaryChannel(HiddenTextChannelId).Overwrites;

        // Assert
        Assert.That(overwrites, Has.Count.EqualTo(1));
        Assert.That(overwrites[0].TargetId, Is.EqualTo(IntegrationGuildId));
        Assert.That(overwrites[0].TargetType, Is.EqualTo(OverwriteTargetType.Role));
        Assert.That(overwrites[0].Permissions.Denied, Is.EqualTo(Permissions.ViewChannels));
        Assert.That(overwrites[0].Permissions.Allowed, Is.EqualTo(Permissions.None));
    }

    // Discord dispatches an update for the parent category when access is gained but not when it is lost,
    // so the tests observing obfuscated state are ordered before the tests that grant access.
    [Test]
    [Order(7)]
    public async Task ObfuscatedChannel_BotGainsAccess_ChannelUpdateDeliversFullMetadata()
    {
        // Arrange
        var expectedChannel = await FetchGuildChannelAsync(HiddenTextChannelId);
        var expectedCategory = await FetchGuildChannelAsync(HiddenCategoryId);
        using var cancellationTokenSource = new CancellationTokenSource(EventTimeout);
        var cancellationToken = cancellationTokenSource.Token;
        var unobfuscatedChannel = WaitForChannelUpdateAsync(HiddenTextChannelId, channel => !channel.IsObfuscated, cancellationToken);
        var unobfuscatedCategory = WaitForChannelUpdateAsync(HiddenCategoryId, channel => !channel.IsObfuscated, cancellationToken);

        // Act
        await GrantSecondaryBotAccessAsync(cancellationToken);
        var updatedChannel = await unobfuscatedChannel;
        var updatedCategory = await unobfuscatedCategory;

        // Assert
        Assert.That(updatedChannel.Name, Is.EqualTo(expectedChannel.Name));
        Assert.That(updatedCategory.Name, Is.EqualTo(expectedCategory.Name));
        Assert.That(GetSecondaryChannel(HiddenTextChannelId).IsObfuscated, Is.False);
    }

    [Test]
    [Order(8)]
    public async Task ObfuscatedChannel_BotLosesAccess_ChannelUpdateObfuscatesMetadata()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource(EventTimeout);
        var cancellationToken = cancellationTokenSource.Token;
        var unobfuscatedChannel = WaitForChannelUpdateAsync(HiddenTextChannelId, channel => !channel.IsObfuscated, cancellationToken);
        await GrantSecondaryBotAccessAsync(cancellationToken);
        await unobfuscatedChannel;

        var reobfuscatedChannel = WaitForChannelUpdateAsync(HiddenTextChannelId, channel => channel.IsObfuscated, cancellationToken);

        // Act
        await RevokeSecondaryBotAccessAsync(cancellationToken);
        var revokedChannel = await reobfuscatedChannel;

        // Assert
        Assert.That(revokedChannel.Name, Is.EqualTo(ObfuscatedName));
        Assert.That(revokedChannel.Overwrites, Has.Count.EqualTo(1));
        Assert.That(GetSecondaryChannel(HiddenTextChannelId).IsObfuscated, Is.True);
    }

    private static IHost CreateSecondaryHost(string token)
    {
        var hostApplicationBuilder = Host.CreateApplicationBuilder();
        hostApplicationBuilder.ConfigureDiscordClient(new DiscordClientHostingContext
        {
            Token = token,
            Intents = GatewayIntents.Guilds,
            ServiceAssemblies = new List<Assembly>()
        });

        hostApplicationBuilder.Services.Configure<DefaultShardConfiguration>(configuration =>
        {
            configuration.Capabilities = GatewayCapabilities.ChannelObfuscation;
        });

        hostApplicationBuilder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        return hostApplicationBuilder.Build();
    }

    private async Task<IGuildChannel> FetchGuildChannelAsync(Snowflake channelId)
    {
        var channel = await RestClient.FetchChannelAsync(channelId);
        Assert.That(channel, Is.InstanceOf<IGuildChannel>(), $"The channel {channelId} is missing from the integration guild.");
        return (IGuildChannel) channel!;
    }

    private CachedGuildChannel GetSecondaryChannel(Snowflake channelId)
    {
        var channel = SecondaryClient.GetChannel(IntegrationGuildId, channelId);
        Assert.That(channel, Is.Not.Null, $"The secondary bot did not receive the channel {channelId}.");
        return channel!;
    }

    private Task GrantSecondaryBotAccessAsync(CancellationToken cancellationToken)
    {
        return RestClient.SetOverwriteAsync(HiddenTextChannelId,
            LocalOverwrite.Member(SecondaryClient.CurrentUser.Id, OverwritePermissions.None.Allow(Permissions.ViewChannels)),
            cancellationToken: cancellationToken);
    }

    private Task RevokeSecondaryBotAccessAsync(CancellationToken cancellationToken)
    {
        return RestClient.DeleteOverwriteAsync(HiddenTextChannelId, SecondaryClient.CurrentUser.Id, cancellationToken: cancellationToken);
    }

    private Task<IGuildChannel> WaitForChannelUpdateAsync(Snowflake channelId, Func<IGuildChannel, bool> predicate, CancellationToken cancellationToken)
    {
        var secondaryClient = SecondaryClient;
        var channelSource = new Tcs<IGuildChannel>();
        secondaryClient.ChannelUpdated += OnChannelUpdated;

        return WaitAsync();

        Task OnChannelUpdated(object? sender, ChannelUpdatedEventArgs e)
        {
            if (e.ChannelId == channelId && predicate(e.NewChannel))
            {
                channelSource.Complete(e.NewChannel);
            }

            return Task.CompletedTask;
        }

        async Task<IGuildChannel> WaitAsync()
        {
            try
            {
                return await channelSource.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw new AssertionException($"The expected update of channel {channelId} was not received before the timeout.");
            }
            finally
            {
                secondaryClient.ChannelUpdated -= OnChannelUpdated;
            }
        }
    }
}
