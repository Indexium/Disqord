﻿using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qommon;

namespace Disqord.Serialization.Json.Default;

/// <summary>
///     Represents a default JSON node.
///     Wraps a <see cref="JToken"/>.
/// </summary>
public class DefaultJsonNode : IJsonNode
{
    /// <summary>
    ///     Gets the underlying <see cref="JToken"/>.
    /// </summary>
    public JToken Token { get; }

    /// <summary>
    ///     Gets the underlying serializer.
    /// </summary>
    public JsonSerializer Serializer { get; }

    public DefaultJsonNode(JToken token, JsonSerializer serializer)
    {
        Token = token;
        Serializer = serializer;
    }

    /// <inheritdoc/>
    public T? ToType<T>()
    {
        return Token.ToObject<T>(Serializer);
    }

    /// <summary>
    ///     Formats this node into a JSON representation with the specified formatting.
    /// </summary>
    /// <param name="formatting"> The formatting to use. </param>
    /// <returns>
    ///     The string representing this node.
    /// </returns>
    public string ToJsonString(JsonFormatting formatting)
    {
        return Token.ToString(formatting switch
        {
            JsonFormatting.Indented => Formatting.Indented,
            _ => Formatting.None
        });
    }

    [return: NotNullIfNotNull("obj")]
    internal static IJsonNode? Create(object? obj, JsonSerializer serializer)
    {
        var token = obj != null ? JToken.FromObject(obj, serializer) : JValue.CreateNull();
        return Create(token, serializer);
    }

    [return: NotNullIfNotNull("token")]
    internal static IJsonNode? Create(JToken? token, JsonSerializer serializer)
    {
        return token switch
        {
            null => null,
            JObject @object => new DefaultJsonObject(@object, serializer),
            JArray array => new DefaultJsonArray(array, serializer),
            JValue value => new DefaultJsonValue(value, serializer),
            _ => throw new InvalidOperationException("Unknown JSON token type.")
        };
    }

    [return: NotNullIfNotNull("node")]
    internal static JToken? GetJToken(IJsonNode? node)
    {
        return node != null
            ? Guard.IsAssignableToType<DefaultJsonNode>(node).Token
            : null;
    }
}
