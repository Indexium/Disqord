﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Disqord.Rest;

public static partial class RestEntityExtensions
{
    public static Task<IApplicationEmoji> ModifyAsync(this IApplicationEmoji emoji,
        Action<ModifyApplicationEmojiActionProperties> action,
        IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var client = emoji.GetRestClient();
        return client.ModifyApplicationEmojiAsync(emoji.ApplicationId, emoji.Id, action, options, cancellationToken);
    }

    public static Task DeleteAsync(this IApplicationEmoji emoji,
        IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var client = emoji.GetRestClient();
        return client.DeleteApplicationEmojiAsync(emoji.ApplicationId, emoji.Id, options, cancellationToken);
    }

    public static Task<IGuildEmoji> ModifyAsync(this IGuildEmoji emoji,
        Action<ModifyGuildEmojiActionProperties> action,
        IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var client = emoji.GetRestClient();
        return client.ModifyGuildEmojiAsync(emoji.GuildId, emoji.Id, action, options, cancellationToken);
    }

    public static Task DeleteAsync(this IGuildEmoji emoji,
        IRestRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var client = emoji.GetRestClient();
        return client.DeleteGuildEmojiAsync(emoji.GuildId, emoji.Id, options, cancellationToken);
    }
}