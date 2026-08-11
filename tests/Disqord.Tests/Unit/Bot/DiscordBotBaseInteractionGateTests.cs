using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands.Interaction;
using Disqord.Bot.Commands.Text;
using Disqord.Gateway;
using Disqord.Gateway.Api;
using Disqord.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Disqord.Tests.Unit.Bot;

public sealed class DiscordBotBaseInteractionGateTests
{
    [Test]
    public async Task ProcessCommandsAsync_AnyInteraction_InvokesOnInteractionCallback()
    {
        // Arrange
        var bot = CreateBot();
        var interaction = NoOpProxy.Create<IUserInteraction>();
        var e = new InteractionReceivedEventArgs(interaction, null);

        // Act
        await bot.ProcessCommandsAsync(e);

        // Assert
        Assert.That(bot.OnInteractionCalls, Has.Count.EqualTo(1));
        Assert.That(bot.OnInteractionCalls[0], Is.SameAs(interaction));
    }

    [Test]
    public async Task ProcessCommandsAsync_OnInteractionReturnsFalse_ReturnsFalseWithoutCreatingCommandContext()
    {
        // Arrange
        var bot = CreateBot();
        bot.OnInteractionResult = false;
        var interaction = NoOpProxy.Create<IUserInteraction>();
        var e = new InteractionReceivedEventArgs(interaction, null);

        // Act
        var result = await bot.ProcessCommandsAsync(e);

        // Assert
        Assert.That(bot.OnInteractionCalls, Has.Count.EqualTo(1));
        Assert.That(result, Is.False);
        Assert.That(bot.ContextCreationAttempted, Is.False);
    }

    private static TestDiscordBot CreateBot()
    {
        var gatewayApiClient = NoOpProxy.Create<IGatewayApiClient>(new Dictionary<string, Func<object?>>
        {
            ["get_ShardCoordinator"] = () => null,
        });
        var gatewayDispatcher = NoOpProxy.Create<IGatewayDispatcher>();
        var gatewayClient = NoOpProxy.Create<IGatewayClient>(new Dictionary<string, Func<object?>>
        {
            ["get_ApiClient"] = () => gatewayApiClient,
            ["get_Dispatcher"] = () => gatewayDispatcher,
        });
        var restClient = NoOpProxy.Create<IRestClient>();

        var client = new DiscordClient(
            Options.Create(new DiscordClientConfiguration()),
            NullLogger<DiscordClient>.Instance,
            restClient,
            gatewayClient,
            Array.Empty<DiscordClientExtension>());

        var services = new FakeServiceProvider(NoOpProxy.Create<IPrefixProvider>(), NoOpProxy.Create<ICommandService>());
        return new TestDiscordBot(services, client);
    }

    private sealed class TestDiscordBot : DiscordBot
    {
        public bool OnInteractionResult { get; set; } = true;

        public List<IUserInteraction> OnInteractionCalls { get; } = new();

        public bool ContextCreationAttempted { get; private set; }

        public TestDiscordBot(IServiceProvider services, DiscordClient client)
            : base(Options.Create(new DiscordBotConfiguration()), NullLogger<DiscordBot>.Instance, services, client)
        { }

        protected override ValueTask<bool> OnInteraction(IUserInteraction interaction)
        {
            OnInteractionCalls.Add(interaction);
            return new ValueTask<bool>(OnInteractionResult);
        }

        public override IDiscordInteractionCommandContext CreateInteractionCommandContext(IUserInteraction interaction)
        {
            ContextCreationAttempted = true;
            throw new InvalidOperationException("Context creation should not be reached in this test.");
        }
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly IPrefixProvider _prefixProvider;
        private readonly ICommandService _commandService;

        public FakeServiceProvider(IPrefixProvider prefixProvider, ICommandService commandService)
        {
            _prefixProvider = prefixProvider;
            _commandService = commandService;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IPrefixProvider))
            {
                return _prefixProvider;
            }

            if (serviceType == typeof(ICommandService))
            {
                return _commandService;
            }

            return null;
        }
    }

    // No-ops void interface members (event add/remove, Bind) and throws for anything else, unless overridden.
    private class NoOpProxy : DispatchProxy
    {
        private Dictionary<string, Func<object?>>? _overrides;

        public static T Create<T>(Dictionary<string, Func<object?>>? overrides = null) where T : class
        {
            var proxy = Create<T, NoOpProxy>();
            ((NoOpProxy) (object) proxy)._overrides = overrides;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                return null;
            }

            if (_overrides != null && _overrides.TryGetValue(targetMethod.Name, out var factory))
            {
                return factory();
            }

            if (targetMethod.ReturnType == typeof(void))
            {
                return null;
            }

            throw new NotSupportedException($"{targetMethod.DeclaringType}.{targetMethod.Name} was not expected to be called in this test.");
        }
    }
}
