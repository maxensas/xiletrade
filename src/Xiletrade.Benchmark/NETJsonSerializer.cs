using System.Text.Encodings.Web;
using System.Text.Json;

namespace Xiletrade.Benchmark;

public sealed class NETJsonSerializer : IJsonSerializer
{
    private JsonSerializerOptions _options;

    public NETJsonSerializer() 
    {
        /* .NET9 AllowCharacters() does nor permit everything sadly.
        var encoderSettings = new TextEncoderSettings();
        encoderSettings.AllowRanges(UnicodeRanges.All);
        encoderSettings.AllowCharacters('\u0022', '\u0027', '\u002B','\u00A0', '\u3000', '\u007f');
        */
        _options = new()
        {
            //WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encodings.web.javascriptencoder.unsaferelaxedjsonescaping
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            //Encoder = JavaScriptEncoder.Create(encoderSettings)
        };
    }
    
    public string Serialize<T>(object obj) where T : class
    {
        /*
        var sb = new System.Text.StringBuilder(JsonSerializer.Serialize(obj, typeof(T), _options));
        sb.Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f");
        return sb.ToString();*/
        return JsonSerializer.Serialize(obj, typeof(T), _options)
            .Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f");
    }

    public T Deserialize<T>(string strData) where T : class
    {
        return strData is not null ? JsonSerializer.Deserialize<T>(strData/*, _options*/) : null;
    }
}