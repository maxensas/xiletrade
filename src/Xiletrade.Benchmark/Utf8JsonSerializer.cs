using Utf8Json;
using Utf8Json.Resolvers;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Benchmark;

/// <summary>
/// seprecated
/// </summary>
public sealed class Utf8JsonSerializer : IJsonSerializer
{
    // formatters of the types written differently from their class (see the json converters of the application)
    private static readonly IJsonFormatterResolver _resolver = CompositeResolver.Create(
        [new IntegerIdFormatter()],
        [StandardResolver.AllowPrivateExcludeNullSnakeCase]);

    public byte[] Serialize<T>(object obj) where T : class
    {
        return JsonSerializer.Serialize((T)obj, _resolver);
    }

    public T Deserialize<T>(byte[] utf8Json) where T : class
    {
        return JsonSerializer.Deserialize<T>(utf8Json, _resolver);
    }
}

/// <summary>
/// IntegerId is a number in the json, not an object : same as IntegerJsonConverter of the application.
/// </summary>
public sealed class IntegerIdFormatter : IJsonFormatter<IntegerId>
{
    public void Serialize(ref JsonWriter writer, IntegerId value, IJsonFormatterResolver formatterResolver)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }
        writer.WriteInt32(value.Id);
    }

    public IntegerId Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
        {
            return null;
        }
        return new() { Id = reader.ReadInt32() };
    }
}
