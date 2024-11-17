using Xiletrade.Benchmark;

namespace Xiletrade.Test.Serialization;

public class UnitTest<T> where T : class
{
    public static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    public static IJsonSerializer Utf8Serializer { get; private set; } = new Utf8JsonSerializer();
    public static IJsonSerializer NETSerializer { get; private set; } = new NETJsonSerializer();

    public Dictionary<string, string> Jsons { get; private set; } = new();
    public Dictionary<string, object> Serializable { get; private set; } = new();

    public UnitTest(string jsonFileName)
    {
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

            var fs = new FileStream(path, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                var json = reader.ReadToEnd();
                var filter = Utf8Serializer.Deserialize<T>(json);

                Jsons.Add(lang, json);
                Serializable.Add(lang, filter);
            }
        }
    }
}
