using Utf8Json;
using Utf8Json.Resolvers;

namespace Xiletrade.Test.Exploratory.Serialization;

/// <summary>
/// Deprecated (old xiletrade serializer)
/// </summary>
public sealed class Utf8JsonSerializer
{
    public byte[] Serialize<T>(object obj) where T : class
    {
        return JsonSerializer.Serialize((T)obj, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    public T Deserialize<T>(byte[] utf8Json) where T : class
    {
        return JsonSerializer.Deserialize<T>(utf8Json, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }
}