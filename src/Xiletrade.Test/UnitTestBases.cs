using Xiletrade.Benchmark;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Test.Comparer;

namespace Xiletrade.Test;

/// <summary>
/// Unit test used to show incorrect Serialization made by System.Text.Json (.NET8) compared to Utf8Json.
/// </summary>
/// <remarks>
/// Will evolve with futur tests / unicode configuration.
/// </remarks>
public class UnitTestBases
{
    public static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    public static Dictionary<string, string> Jsons { get; private set; } = new();
    public static Dictionary<string, BaseData> Bases { get; private set; } = new();
    public static IJsonSerializer Utf8Serializer { get; private set; } = new Utf8JsonSerializer();
    public static IJsonSerializer NETSerializer { get; private set; } = new NETJsonSerializer();

    public UnitTestBases()
    {
        if (Jsons.Count > 0 || Bases.Count > 0)
        {
            return;
        }

        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = string.Concat(basePath.AsSpan(0, basePath.IndexOf("bin")), "Data\\Lang\\");
        foreach (var lang in Culture)
        {
            var path = basePath + lang + "\\Bases.json";

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
                var filter = Utf8Serializer.Deserialize<BaseData>(json);

                Jsons.Add(lang, json);
                Bases.Add(lang, filter);
            }
        }
    }

    [Fact]
    public void _00_EN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[0]));

        Assert.Equal(Jsons.GetValueOrDefault(Culture[0]), net);
    }

    [Fact]
    public void _01_KR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[1]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[1]), net);
    }

    [Fact]
    public void _02_FR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[2]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[2]), net);
    }

    [Fact]
    public void _03_ES_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[3]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[3]), net);
    }

    [Fact]
    public void _04_DE_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[4]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[4]), net);
    }

    [Fact]
    public void _05_BR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[5]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[5]), net);
    }

    [Fact]
    public void _06_RU_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[6]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[6]), net);
    }
    [Fact]
    public void _07_TH_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[7]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[7]), net);
    }
    [Fact]
    public void _08_TW_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[8]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[8]), net);
    }
    [Fact]
    public void _09_CN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[9]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[9]), net);
    }
    [Fact]
    public void _10_JP_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<BaseData>(Bases.GetValueOrDefault(Culture[10]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[10]), net);
    }
}