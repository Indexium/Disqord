using System;

namespace Disqord.Bot.Commands.Components;

/// <summary>
///     Marks the decorated modal command to bind its non-route arguments by matching each parameter's
///     name against the custom ID of a modal component, instead of the default behaviour of binding
///     them positionally in declaration order.
/// </summary>
/// <remarks>
///     Every parameter targeted by this binding mode must have a modal component whose custom ID
///     equals the parameter's name, or binding fails.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class BindModalArgumentsByCustomIdAttribute : Attribute
{ }
