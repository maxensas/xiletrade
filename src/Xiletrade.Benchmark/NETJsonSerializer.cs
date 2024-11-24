using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Benchmark;

/// <remarks>
/// Utf8Json is still better compared to .NET9 Serializer.
/// </remarks>
public sealed class NETJsonSerializer : IJsonSerializer
{
    public string Serialize<T>(object obj) where T : class
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), SourceGenerationContext.ContextWithOptions)
            .Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f")
            .Replace("\\u0022", "\\\"").Replace("\\u0027", "\u0027"); //.Replace("\\u0026", "\u0026")
    }

    public T Deserialize<T>(string strData) where T : class
    {
        return System.Text.Json.JsonSerializer.Deserialize(strData, typeof(T), SourceGenerationContext.ContextWithOptions) as T;
    }
}