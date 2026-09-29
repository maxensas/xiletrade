using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Xiletrade.Test.Serialization;

/// <summary>
/// Deprecated (old xiletrade serializer)
/// </summary>
public sealed class Utf8JsonSerializer
{
    public string Serialize<T>(object obj) where T : class
    {
        return JsonSerializer.ToJsonString((T)obj, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    public T Deserialize<T>(string strData) where T : class
    {
        byte[] data = Encoding.UTF8.GetBytes(strData);
        return JsonSerializer.Deserialize<T>(data, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }
}