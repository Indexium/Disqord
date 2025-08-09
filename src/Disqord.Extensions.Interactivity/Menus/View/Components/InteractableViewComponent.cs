﻿using System.Threading.Tasks;
using Disqord.Gateway;

namespace Disqord.Extensions.Interactivity.Menus;

public abstract class InteractableViewComponent : ViewComponent
{
    public string CustomId { get; set; } = null!;

    protected InteractableViewComponent()
    { }

    protected InteractableViewComponent(ComponentAttribute attribute)
        : base(attribute)
    { }

    protected internal abstract ValueTask ExecuteAsync(InteractionReceivedEventArgs e);

}
