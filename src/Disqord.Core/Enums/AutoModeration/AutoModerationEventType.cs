namespace Disqord;

/// <summary>
///     Represents events that can be checked by auto-moderation rules.
/// </summary>
public enum AutoModerationEventType
{
    /// <summary>
    ///     Represents when a message is sent or edited.
    /// </summary>
    MessageSent = 1,

    /// <summary>
    ///     Represents when a member joins the guild or updates their profile.
    /// </summary>
    MemberUpdated = 2,
}
