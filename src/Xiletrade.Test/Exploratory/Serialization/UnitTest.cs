using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Services;
using Xiletrade.Test.Configuration;

namespace Xiletrade.Test.Exploratory.Serialization;

public class UnitTest<T> : LibServiceConfiguration where T : class
{
    public static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    public static Utf8JsonSerializer Utf8Serializer { get; private set; } = new();
    public static JsonHelper NETSerializer { get; private set; } //= new();
    // TO TRY when AOT compatibility will be added : 'REDox.Json' from CAPCOM https://github.com/CAPCOM-TD-OSS/REDox

    public Dictionary<string, byte[]> Jsons { get; private set; } = new();
    public Dictionary<string, object> Serializable { get; private set; } = new();

    public UnitTest(string jsonFileName)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        NETSerializer = scope.ServiceProvider.GetRequiredService<DataManagerService>().Json;

        if (Jsons.Count > 0 || Serializable.Count > 0)
        {
            return;
        }

        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = string.Concat(basePath.AsSpan(0, basePath.IndexOf("bin")), "Data\\Lang\\");
        foreach (var lang in Culture)
        {
            var path = basePath + lang + "\\" + jsonFileName + ".json";

            if (!File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine("File not found : " + path);
                return;
            }

            var json = File.ReadAllBytes(path);
            if (json.AsSpan().StartsWith(Encoding.UTF8.Preamble))
            {
                json = json[Encoding.UTF8.Preamble.Length..];
            }
            var filter = Utf8Serializer.Deserialize<T>(json);

            Jsons.Add(lang, json);
            Serializable.Add(lang, filter);
        }
    }

    /// <summary>Replaces a text in an UTF-8 json.</summary>
    protected static byte[] Replace(ReadOnlySpan<byte> json, string oldValue, string newValue)
        => Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(json).Replace(oldValue, newValue));
}
