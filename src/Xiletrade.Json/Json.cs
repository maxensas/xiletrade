using Xiletrade.Library.Models.Serializable.SourceGeneration;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xiletrade.Json;

// After testing and bench System.Text.Json from .NET7/8/9 on this project :
// Serialization is still better with Utf8Json library but System.Text.Json allow native AOT with SourceGenerationContext.
// Utf8Json keep unicode characters of each language and serialize all symbols correctly.
internal static class Json 
{
    internal static string Serialize<T>(object obj) where T : class
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), SourceGenerationContext.ContextWithOptions)
            .Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f")
            .Replace("\\u0022", "\\\"").Replace("\\u0027", "\u0027"); //.Replace("\\u0026", "\u0026")
    }

    internal static T? Deserialize<T>(string strData) where T : class
    {
        return System.Text.Json.JsonSerializer.Deserialize(strData, typeof(T), SourceGenerationContext.ContextWithOptions) as T;
    }
    
    /*
    internal static string Serialize<T>(object obj) where T : class
    {
        return Utf8Json.JsonSerializer.ToJsonString((T)obj, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    internal static T Deserialize<T>(string strData) where T : class
    {
        byte[] data = Encoding.UTF8.GetBytes(strData);
        return Utf8Json.JsonSerializer.Deserialize<T>(data, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }
    */
}
