using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

namespace XiletradeJson
{
    // After testing System.Text.Json .NET7 on this project : it's better to use Utf8Json library in order to keep characters of each language. 
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
