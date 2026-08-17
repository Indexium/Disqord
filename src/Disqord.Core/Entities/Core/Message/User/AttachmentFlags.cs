using System;

namespace Disqord;

[Flags]
public enum AttachmentFlags
{
    /// <summary>
    ///     The attachment has no flags.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The attachment is a clip.
    /// </summary>
    Clip = 1 << 0,

    /// <summary>
    ///     The attachment is a thumbnail.
    /// </summary>
    Thumbnail = 1 << 1,

    /// <summary>
    ///     The attachment has been edited using the remix feature.
    /// </summary>
    Remix = 1 << 2,

    /// <summary>
    ///     The attachment is marked as a spoiler.
    /// </summary>
    Spoiler = 1 << 3,

    /// <summary>
    ///     The attachment is an animated image.
    /// </summary>
    Animated = 1 << 5,
}
