using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Qommon.Serialization;

namespace Disqord.Serialization.Json.Default;

internal sealed class OptionalConverter : JsonConverter
{
    public static readonly OptionalConverter Instance = new(null);

    private readonly JsonConverter? _converter;

    private OptionalConverter(JsonConverter? converter)
    {
        _converter = converter;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    private static readonly ConcurrentDictionary<Type, ConstructorInfo> _cache =
        new ConcurrentDictionary<Type, ConstructorInfo>();

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var constructor = _cache.GetOrAdd(objectType, t => t.GetConstructors().First());

        var genericType = objectType.GenericTypeArguments.FirstOrDefault();
        if (genericType == null)
            throw new InvalidOperationException($"No generic type arguments found for type {objectType.Name}");

        return constructor.Invoke([serializer.Deserialize(reader, genericType)]);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var optionalValue = ((IOptional?) value)!.Value;
        if (optionalValue == null)
        {
            writer.WriteNull();
        }
        else
        {
            if (_converter != null)
            {
                _converter.WriteJson(writer, optionalValue, serializer);
            }
            else
            {
                serializer.Serialize(writer, optionalValue);
            }
        }
    }

    public static OptionalConverter Create(JsonConverter? converter = null)
    {
        return converter != null
            ? new OptionalConverter(converter)
            : Instance;
    }
}
