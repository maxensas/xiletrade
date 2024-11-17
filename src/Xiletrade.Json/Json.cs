using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Xiletrade.Json
{
    // After testing and bench System.Text.Json from .NET7/8/9 on this project :
    // Serialization is still better with Utf8Json library.
    // It keep unicode characters of each language and serialize all symbols correctly.
    internal static class Json 
    {
        internal static string Serialize<T>(object obj) where T : class
        {
            return JsonSerializer.ToJsonString((T)obj, StandardResolver.AllowPrivateExcludeNullSnakeCase);
        }

        internal static T Deserialize<T>(string strData) where T : class
        {
            byte[] data = Encoding.UTF8.GetBytes(strData);
            return JsonSerializer.Deserialize<T>(data, StandardResolver.AllowPrivateExcludeNullSnakeCase);
        }
    }
}
