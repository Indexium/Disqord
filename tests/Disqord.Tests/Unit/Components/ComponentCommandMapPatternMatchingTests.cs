using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Commands.Components;
using Qmmands;

namespace Disqord.Tests.Unit.Components;

public class ComponentCommandMapPatternMatchingTests
{
    [Test]
    public void FindCommand_PatternWithEmptyWildcardSegment_MatchesAndCapturesEmptyValue()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var command = CreateCommand("edit:*:*");
        node.AddCommand(command);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit::42", out var rawArguments);

        // Assert
        Assert.That(found, Is.SameAs(command));
        var captures = rawArguments!.Select(multiString => multiString[0].ToString()).ToArray();
        Assert.That(captures, Is.EqualTo(new[] { "", "42" }));
    }

    [Test]
    public void FindCommand_PatternWithoutEmptySegments_StillMatchesNormally()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var command = CreateCommand("edit:*:*");
        node.AddCommand(command);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:foo:42", out var rawArguments);

        // Assert
        Assert.That(found, Is.SameAs(command));
        var captures = rawArguments!.Select(multiString => multiString[0].ToString()).ToArray();
        Assert.That(captures, Is.EqualTo(new[] { "foo", "42" }));
    }

    private static ComponentCommand CreateCommand(string pattern)
    {
        var moduleBuilder = new ComponentModuleBuilder();
        var module = moduleBuilder.Build();
        var commandBuilder = new ComponentCommandBuilder(moduleBuilder, new FakeCallback())
        {
            Pattern = pattern,
            Type = ComponentCommandType.Button
        };

        return commandBuilder.Build(module);
    }

    private sealed class FakeCallback : ICommandCallback
    {
        public ValueTask<IModuleBase?> CreateModuleBase(ICommandContext context)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<IResult?> ExecuteAsync(ICommandContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
