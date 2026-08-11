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

    [Test]
    public void FindCommand_WildcardRegisteredBeforeSpecificPattern_MatchesTheSpecificPattern()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var wildcardCommand = CreateCommand("edit:*");
        var specificCommand = CreateCommand("edit:confirm");
        node.AddCommand(wildcardCommand);
        node.AddCommand(specificCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:confirm", out _);

        // Assert
        Assert.That(found, Is.SameAs(specificCommand));
    }

    [Test]
    public void FindCommand_SpecificPatternRegisteredBeforeWildcard_StillMatchesTheSpecificPattern()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var specificCommand = CreateCommand("edit:confirm");
        var wildcardCommand = CreateCommand("edit:*");
        node.AddCommand(specificCommand);
        node.AddCommand(wildcardCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:confirm", out _);

        // Assert
        Assert.That(found, Is.SameAs(specificCommand));
    }

    [Test]
    public void FindCommand_OnlyWildcardMatches_MatchesTheWildcard()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var wildcardCommand = CreateCommand("edit:*");
        var specificCommand = CreateCommand("edit:confirm");
        node.AddCommand(wildcardCommand);
        node.AddCommand(specificCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:cancel", out _);

        // Assert
        Assert.That(found, Is.SameAs(wildcardCommand));
    }

    [Test]
    public void FindCommand_PatternWithFewerWildcardsBeatsPatternWithMoreWildcards_WildcardsRegisteredFirst()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var twoWildcardsCommand = CreateCommand("edit:*:*");
        var oneWildcardCommand = CreateCommand("edit:confirm:*");
        node.AddCommand(twoWildcardsCommand);
        node.AddCommand(oneWildcardCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:confirm:42", out _);

        // Assert
        Assert.That(found, Is.SameAs(oneWildcardCommand));
    }

    [Test]
    public void FindCommand_PatternWithFewerWildcardsBeatsPatternWithMoreWildcards_MoreSpecificRegisteredFirst()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var oneWildcardCommand = CreateCommand("edit:confirm:*");
        var twoWildcardsCommand = CreateCommand("edit:*:*");
        node.AddCommand(oneWildcardCommand);
        node.AddCommand(twoWildcardsCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:confirm:42", out _);

        // Assert
        Assert.That(found, Is.SameAs(oneWildcardCommand));
    }

    [Test]
    public void FindCommand_OnlyPatternWithMoreWildcardsMatches_FallsBackToIt()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var twoWildcardsCommand = CreateCommand("edit:*:*");
        var oneWildcardCommand = CreateCommand("edit:confirm:*");
        node.AddCommand(twoWildcardsCommand);
        node.AddCommand(oneWildcardCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:cancel:42", out _);

        // Assert
        Assert.That(found, Is.SameAs(twoWildcardsCommand));
    }

    [Test]
    public void FindCommand_TiedWildcardCountAtDifferentPositions_KeepsFirstRegisteredPattern()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var leadingLiteralCommand = CreateCommand("a:*");
        var trailingLiteralCommand = CreateCommand("*:b");
        node.AddCommand(leadingLiteralCommand);
        node.AddCommand(trailingLiteralCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "a:b", out _);

        // Assert
        Assert.That(found, Is.SameAs(leadingLiteralCommand));
    }

    [Test]
    public void FindCommand_NonMatchingLowerWildcardPatternRegisteredFirst_StillFindsLaterHigherWildcardMatch()
    {
        // Arrange
        var node = new ComponentCommandMap.Node();
        var nonMatchingSpecificCommand = CreateCommand("edit:confirm");
        var matchingWildcardCommand = CreateCommand("edit:*");
        node.AddCommand(nonMatchingSpecificCommand);
        node.AddCommand(matchingWildcardCommand);

        // Act
        var found = node.FindCommand(ComponentCommandType.Button, "edit:cancel", out _);

        // Assert
        Assert.That(found, Is.SameAs(matchingWildcardCommand));
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
