using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands.Components;
using Disqord.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Qmmands;
using Qmmands.Default;
using Qommon;

namespace Disqord.Tests.Unit.Components;

public sealed class ModalOptionalGroupTypeParseCrashReproTests
{
    [Test]
    public void GetRawArgumentFromModalComponent_UnansweredRadioGroup_ProducesZeroCountMultiString()
    {
        // Arrange
        var bindValues = new ExposedBindValues();
        var model = new ModalRadioGroupComponentJsonModel
        {
            CustomId = "radio",
        };
        var radioGroupComponent = new TransientModalRadioGroupComponent(model);

        // Act
        var rawArgument = bindValues.Invoke(radioGroupComponent);

        // Assert
        Assert.That(rawArgument.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetRawArgumentFromModalComponent_UnansweredCheckboxGroup_ProducesZeroCountMultiString()
    {
        // Arrange
        var bindValues = new ExposedBindValues();
        var model = new ModalCheckboxGroupComponentJsonModel
        {
            CustomId = "checkboxes",
            Values = Array.Empty<string>(),
        };
        var checkboxGroupComponent = new TransientModalCheckboxGroupComponent(model);

        // Act
        var rawArgument = bindValues.Invoke(checkboxGroupComponent);

        // Assert
        Assert.That(rawArgument.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task TypeParse_ZeroCountRawArgumentForTypedParameter_ThrowsInvalidOperationException()
    {
        // Arrange
        var moduleBuilder = new ComponentModuleBuilder();
        var module = moduleBuilder.Build();
        var commandBuilder = new ComponentCommandBuilder(moduleBuilder, new FakeCallback())
        {
            Pattern = "test",
            Type = ComponentCommandType.Button,
        };
        var parameterBuilder = new ComponentParameterBuilder(commandBuilder, typeof(int))
        {
            Name = "amount",
        };
        commandBuilder.Parameters.Add(parameterBuilder);
        var command = commandBuilder.Build(module);
        var parameter = command.Parameters[0];

        var typeParserProvider = new DefaultTypeParserProvider(
            Options.Create(new DefaultTypeParserServiceConfiguration()),
            NullLogger<DefaultTypeParserProvider>.Instance);

        var context = new FakeCommandContext(new FakeServiceProvider(typeParserProvider))
        {
            Command = command,
            RawArguments = new Dictionary<IParameter, MultiString>
            {
                [parameter] = default,
            },
        };

        ICommandExecutionStep typeParseStep = new DefaultExecutionSteps.TypeParse();

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await typeParseStep.ExecuteAsync(context);
        });
        Assert.That(exception!.Message, Does.Contain("must not be a null instance"));
    }

    [Test]
    public void BindArgumentFromModalComponent_UnansweredRadioGroupBoundToIntParameter_DoesNotSetRawArgument()
    {
        // Arrange
        var (bindValues, parameter, context) = CreateIntParameterFixture(isOptional: false);
        var model = new ModalRadioGroupComponentJsonModel
        {
            CustomId = "rating",
        };
        var radioGroupComponent = new TransientModalRadioGroupComponent(model);

        // Act
        bindValues.InvokeBindArgument(context, parameter, radioGroupComponent);

        // Assert
        Assert.That(context.RawArguments!.ContainsKey(parameter), Is.False);
    }

    [Test]
    public void BindArgumentFromModalComponent_UnansweredCheckboxGroupBoundToIntArrayParameter_DoesNotSetRawArgument()
    {
        // Arrange
        var (bindValues, parameter, context) = CreateIntArrayParameterFixture(isOptional: false);
        var model = new ModalCheckboxGroupComponentJsonModel
        {
            CustomId = "checkboxes",
            Values = Array.Empty<string>(),
        };
        var checkboxGroupComponent = new TransientModalCheckboxGroupComponent(model);

        // Act
        bindValues.InvokeBindArgument(context, parameter, checkboxGroupComponent);

        // Assert
        Assert.That(context.RawArguments!.ContainsKey(parameter), Is.False);
    }

    [Test]
    public async Task BindArgumentFromModalComponent_UnansweredRadioGroupBoundToRequiredIntParameter_ArgumentBinderFailsWithoutThrowing()
    {
        // Arrange
        var (bindValues, parameter, context) = CreateIntParameterFixture(isOptional: false);
        var model = new ModalRadioGroupComponentJsonModel
        {
            CustomId = "rating",
        };
        var radioGroupComponent = new TransientModalRadioGroupComponent(model);
        bindValues.InvokeBindArgument(context, parameter, radioGroupComponent);
        var typeParseStep = new DefaultExecutionSteps.TypeParse
        {
            Next = new TerminalExecutionStep(),
        };
        await typeParseStep.ExecuteAsync(context);

        // Act
        var bindResult = await new DefaultArgumentBinder().BindAsync(context);

        // Assert
        Assert.That(bindResult.IsSuccessful, Is.False);
    }

    [Test]
    public async Task BindArgumentFromModalComponent_UnansweredRadioGroupBoundToOptionalIntParameterWithDefault_ArgumentBinderSucceedsWithDefault()
    {
        // Arrange
        var (bindValues, parameter, context) = CreateIntParameterFixture(isOptional: true);
        var model = new ModalRadioGroupComponentJsonModel
        {
            CustomId = "rating",
        };
        var radioGroupComponent = new TransientModalRadioGroupComponent(model);
        bindValues.InvokeBindArgument(context, parameter, radioGroupComponent);
        var typeParseStep = new DefaultExecutionSteps.TypeParse
        {
            Next = new TerminalExecutionStep(),
        };
        await typeParseStep.ExecuteAsync(context);

        // Act
        var bindResult = await new DefaultArgumentBinder().BindAsync(context);

        // Assert
        Assert.That(bindResult.IsSuccessful, Is.True);
        Assert.That(context.Arguments![parameter], Is.EqualTo(0));
    }

    [Test]
    public async Task BindArgumentFromModalComponent_UnansweredCheckboxGroupBoundToRequiredIntArrayParameter_ArgumentBinderFailsWithoutThrowing()
    {
        // Arrange
        var (bindValues, parameter, context) = CreateIntArrayParameterFixture(isOptional: false);
        var model = new ModalCheckboxGroupComponentJsonModel
        {
            CustomId = "checkboxes",
            Values = Array.Empty<string>(),
        };
        var checkboxGroupComponent = new TransientModalCheckboxGroupComponent(model);
        bindValues.InvokeBindArgument(context, parameter, checkboxGroupComponent);
        var typeParseStep = new DefaultExecutionSteps.TypeParse
        {
            Next = new TerminalExecutionStep(),
        };
        await typeParseStep.ExecuteAsync(context);

        // Act
        var bindResult = await new DefaultArgumentBinder().BindAsync(context);

        // Assert
        Assert.That(bindResult.IsSuccessful, Is.False);
    }

    private static DefaultTypeParserProvider CreateTypeParserProvider()
    {
        return new DefaultTypeParserProvider(
            Options.Create(new DefaultTypeParserServiceConfiguration()),
            NullLogger<DefaultTypeParserProvider>.Instance);
    }

    private static (ExposedBindValues BindValues, IParameter Parameter, FakeComponentCommandContext Context) CreateIntParameterFixture(bool isOptional)
    {
        var moduleBuilder = new ComponentModuleBuilder();
        var module = moduleBuilder.Build();
        var commandBuilder = new ComponentCommandBuilder(moduleBuilder, new FakeCallback())
        {
            Pattern = "survey",
            Type = ComponentCommandType.Modal,
        };
        var parameterBuilder = new ComponentParameterBuilder(commandBuilder, typeof(int))
        {
            Name = "rating",
        };
        if (isOptional)
        {
            parameterBuilder.DefaultValue = 0;
        }

        commandBuilder.Parameters.Add(parameterBuilder);
        var command = commandBuilder.Build(module);
        var parameter = command.Parameters[0];

        var context = new FakeComponentCommandContext(new FakeServiceProvider(CreateTypeParserProvider()))
        {
            Command = command,
            RawArguments = new Dictionary<IParameter, MultiString>(),
            Arguments = new Dictionary<IParameter, object?>(),
        };

        return (new ExposedBindValues(), parameter, context);
    }

    private static (ExposedBindValues BindValues, IParameter Parameter, FakeComponentCommandContext Context) CreateIntArrayParameterFixture(bool isOptional)
    {
        var moduleBuilder = new ComponentModuleBuilder();
        var module = moduleBuilder.Build();
        var commandBuilder = new ComponentCommandBuilder(moduleBuilder, new FakeCallback())
        {
            Pattern = "survey",
            Type = ComponentCommandType.Modal,
        };
        var parameterBuilder = new ComponentParameterBuilder(commandBuilder, typeof(int[]))
        {
            Name = "picks",
        };
        if (isOptional)
        {
            parameterBuilder.DefaultValue = Array.Empty<int>();
        }

        commandBuilder.Parameters.Add(parameterBuilder);
        var command = commandBuilder.Build(module);
        var parameter = command.Parameters[0];

        var context = new FakeComponentCommandContext(new FakeServiceProvider(CreateTypeParserProvider()))
        {
            Command = command,
            RawArguments = new Dictionary<IParameter, MultiString>(),
            Arguments = new Dictionary<IParameter, object?>(),
        };

        return (new ExposedBindValues(), parameter, context);
    }

    private sealed class ExposedBindValues : DefaultComponentExecutionSteps.BindValues
    {
        public MultiString Invoke(IModalComponent modalComponent)
        {
            var method = typeof(DefaultComponentExecutionSteps.BindValues).GetMethod(
                "GetRawArgumentFromModalComponent",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return (MultiString) method!.Invoke(this, new object[] { modalComponent })!;
        }

        public void InvokeBindArgument(IDiscordComponentCommandContext context, IParameter parameter, IModalComponent modalComponent)
        {
            var method = typeof(DefaultComponentExecutionSteps.BindValues).GetMethod(
                "BindArgumentFromModalComponent",
                BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                method!.Invoke(this, new object[] { context, parameter, modalComponent });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }
    }

    private sealed class TerminalExecutionStep : ICommandExecutionStep
    {
        public ICommandExecutionStep Next { get; set; } = null!;

        public ValueTask<IResult> ExecuteAsync(ICommandContext context)
        {
            return Results.Success;
        }
    }

    private sealed class FakeComponentCommandContext : IDiscordComponentCommandContext
    {
        public IServiceProvider Services { get; }

        public CancellationToken CancellationToken => CancellationToken.None;

        public CultureInfo Locale => CultureInfo.InvariantCulture;

        public ICommandExecutionStep? ExecutionStep { get; set; }

        public ICommand? Command { get; set; }

        public IDictionary<IParameter, MultiString>? RawArguments { get; set; }

        public IDictionary<IParameter, object?>? Arguments { get; set; }

        public IModuleBase? ModuleBase { get; set; }

        public DiscordBotBase Bot => throw new NotSupportedException();

        public CultureInfo? GuildLocale => null;

        public Snowflake? GuildId => null;

        public Snowflake ChannelId => default;

        public IUser Author => throw new NotSupportedException();

        public IUserInteraction Interaction { get; set; } = null!;

        public FakeComponentCommandContext(IServiceProvider services)
        {
            Services = services;
        }

        public ValueTask ResetAsync()
        {
            return default;
        }
    }

    private sealed class FakeCallback : ICommandCallback
    {
        public ValueTask<IModuleBase?> CreateModuleBase(ICommandContext context)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IResult?> ExecuteAsync(ICommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly ITypeParserProvider _typeParserProvider;

        public FakeServiceProvider(ITypeParserProvider typeParserProvider)
        {
            _typeParserProvider = typeParserProvider;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ITypeParserProvider))
            {
                return _typeParserProvider;
            }

            return null;
        }
    }

    private sealed class FakeCommandContext : ICommandContext
    {
        public IServiceProvider Services { get; }

        public CancellationToken CancellationToken => CancellationToken.None;

        public CultureInfo Locale => CultureInfo.InvariantCulture;

        public ICommandExecutionStep? ExecutionStep { get; set; }

        public ICommand? Command { get; set; }

        public IDictionary<IParameter, MultiString>? RawArguments { get; set; }

        public IDictionary<IParameter, object?>? Arguments { get; set; }

        public IModuleBase? ModuleBase { get; set; }

        public FakeCommandContext(IServiceProvider services)
        {
            Services = services;
        }

        public ValueTask ResetAsync()
        {
            return default;
        }
    }

}
