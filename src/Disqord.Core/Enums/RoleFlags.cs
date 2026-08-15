using System;

namespace Disqord;

/// <summary>
///     Represents the flags of a role.
/// </summary>
[Flags]
public enum RoleFlags
{
    /// <summary>
    ///     The role can be selected by members in an onboarding prompt.
    /// </summary>
    InPrompt = 1 << 0,
}
