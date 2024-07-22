using System.Text.Json;

namespace Xiletrade.Benchmark;

public sealed class NETJsonSerializer : IJsonSerializer
{
    private JsonSerializerOptions _options;

    public NETJsonSerializer() 
    {
        _options = new()
        {
            //WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encodings.web.javascriptencoder.unsaferelaxedjsonescaping
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping

            //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) 
            //other method System.Text.RegularExpressions.Regex.Unescape()
        };
    }
    
    public string Serialize<T>(object obj) where T : class
    {
        return JsonSerializer.Serialize(obj, typeof(T), _options);
    }

    public T Deserialize<T>(string strData) where T : class
    {
        return strData is not null ? JsonSerializer.Deserialize<T>(strData/*, _options*/) : null;
    }
}