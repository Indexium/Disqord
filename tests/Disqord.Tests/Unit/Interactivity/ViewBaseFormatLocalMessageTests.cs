using System.Linq;
using Disqord.Extensions.Interactivity.Menus;

namespace Disqord.Tests.Unit.Interactivity;

public class ViewBaseFormatLocalMessageTests
{
    [Test]
    public void FormatLocalMessage_TemplateSetsComponents_ViewAppendsItsOwnComponentsInsteadOfOverwriting()
    {
        // Arrange
        var view = new TestView(message =>
        {
            message.Components = new LocalComponent[] { LocalComponent.TextDisplay("template") };
        });

        view.AddComponent(new ButtonViewComponent(_ => default)
        {
            Label = "view-button"
        });

        var message = new LocalMessage();

        // Act
        view.FormatLocalMessage(message);

        // Assert
        var components = message.Components.Value;
        Assert.That(components, Has.Count.EqualTo(2));
        Assert.That(components[0], Is.TypeOf<LocalTextDisplayComponent>());
        Assert.That(components[1], Is.TypeOf<LocalRowComponent>());
    }

    private sealed class TestView : ViewBase
    {
        public TestView(System.Action<LocalMessageBase> messageTemplate)
            : base(messageTemplate)
        { }
    }
}
