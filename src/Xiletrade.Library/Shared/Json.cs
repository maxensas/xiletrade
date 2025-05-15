using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Models.Serializable.SourceGeneration;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Shared;

/// <summary>Static helper class used for JSON serialization.</summary>
/// <remarks>using System.Text.Json</remarks>
internal static class Json
{
    internal static string Serialize<T>(object obj) where T : class
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), SourceGenerationContext.ContextWithOptions)
            .Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f")
            .Replace("\\u0022", "\\\"").Replace("\\u0027", "\u0027"); //.Replace("\\u0026", "\u0026")
    }

    internal static T Deserialize<T>(string strData) where T : class
    {
        return System.Text.Json.JsonSerializer.Deserialize(strData, typeof(T), SourceGenerationContext.ContextWithOptions) as T;
    }

    internal static string GetSerialized(DataManagerService dm, XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        if (isPoe2)
        {
            var jsonDataTwo = new JsonDataTwo(dm, xiletradeItem, item, useSaleType, market);
            return Serialize<JsonDataTwo>(jsonDataTwo);
        }
        var jsonData = new JsonData(dm, xiletradeItem, item, useSaleType, market);
        return Serialize<JsonData>(jsonData);
    }

    /*
    // Old method will be removed.
    internal static string SerializeOld<T>(object obj) where T : class
    {
        return Utf8Json.JsonSerializer.ToJsonString((T)obj, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    // Old method will be removed.
    internal static T DeserializeOld<T>(string strData) where T : class
    {
        byte[] data = Encoding.UTF8.GetBytes(strData);
        return Utf8Json.JsonSerializer.Deserialize<T>(data, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }
    */
}
