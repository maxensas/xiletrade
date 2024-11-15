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
public class UnitTestFilters
{
    public static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    public static Dictionary<string, string> Jsons { get; private set; } = new();
    public static Dictionary<string, FilterData> Filters { get; private set; } = new();
    public static IJsonSerializer Utf8Serializer { get; private set; } = new Utf8JsonSerializer();
    public static IJsonSerializer NETSerializer { get; private set; } = new NETJsonSerializer();

    public UnitTestFilters()
    {
        if (Jsons.Count > 0 || Filters.Count > 0)
        {
            return;
        }

        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = string.Concat(basePath.AsSpan(0, basePath.IndexOf("bin")), "Data\\Lang\\");
        foreach (var lang in Culture)
        {
            var path = basePath + lang + "\\Filters.json";

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
                var filter = Utf8Serializer.Deserialize<FilterData>(json);

                Jsons.Add(lang, json);
                Filters.Add(lang, filter);
            }
        }
    }

    [Fact]
    public void _00_EN_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[0]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[0]), utf8);
    }

    [Fact]
    public void _00_EN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[0]));

        Assert.Equal(Jsons.GetValueOrDefault(Culture[0]), net);
    }

    [Fact]
    public void _00_EN_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[0]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[0]), net, new FilterDataComparer());
    }

    [Fact]
    public void _00_EN_DeserializeWithNETCorruptOne()
    {
        var jsonCorrupted = Jsons.GetValueOrDefault(Culture[0]).Replace("explicit", "tghfghf");
        var net = NETSerializer.Deserialize<FilterData>(jsonCorrupted);
        Assert.NotEqual(Filters.GetValueOrDefault(Culture[0]), net, new FilterDataComparer());
    }

    [Fact]
    public void _00_EN_DeserializeWithNETCorruptTwo()
    {
        var jsonCorrupted = Jsons.GetValueOrDefault(Culture[0]).Replace("crucible.mod_10387", "crucible.dom_10387");
        var net = NETSerializer.Deserialize<FilterData>(jsonCorrupted);
        Assert.NotEqual(Filters.GetValueOrDefault(Culture[0]), net, new FilterDataComparer());
    }

    [Fact]
    public void _01_KR_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[1]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[1]), utf8);
    }

    [Fact]
    public void _01_KR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[1]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[1]), net);
    }

    [Fact]
    public void _01_KR_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[1]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[1]), net, new FilterDataComparer());
    }

    [Fact]
    public void _02_FR_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[2]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[2]), utf8);
    }

    [Fact]
    public void _02_FR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[2]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[2]), net);
    }

    [Fact]
    public void _02_FR_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[2]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[2]), net, new FilterDataComparer());
    }

    [Fact]
    public void _03_ES_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[3]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[3]), utf8);
    }

    [Fact]
    public void _03_ES_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[3]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[3]), net);
    }

    [Fact]
    public void _03_ES_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[3]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[3]), net, new FilterDataComparer());
    }

    [Fact]
    public void _04_DE_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[4]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[4]), utf8);
    }

    [Fact]
    public void _04_DE_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[4]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[4]), net);
    }

    [Fact]
    public void _04_DE_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[4]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[4]), net, new FilterDataComparer());
    }

    [Fact]
    public void _05_BR_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[5]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[5]), utf8);
    }

    [Fact]
    public void _05_BR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[5]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[5]), net);
    }

    [Fact]
    public void _05_BR_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[5]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[5]), net, new FilterDataComparer());
    }

    [Fact]
    public void _06_RU_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[6]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[6]), utf8);
    }

    [Fact]
    public void _06_RU_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[6]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[6]), net);
    }

    [Fact]
    public void _06_RU_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[6]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[6]), net, new FilterDataComparer());
    }

    [Fact]
    public void _07_TH_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[7]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[7]), utf8);
    }

    [Fact]
    public void _07_TH_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[7]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[7]), net);
    }

    [Fact]
    public void _07_TH_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[7]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[7]), net, new FilterDataComparer());
    }

    [Fact]
    public void _08_TW_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[8]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[8]), utf8);
    }

    [Fact]
    public void _08_TW_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[8]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[8]), net);
    }

    [Fact]
    public void _08_TW_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[8]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[8]), net, new FilterDataComparer());
    }

    [Fact]
    public void _09_CN_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[9]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[9]), utf8);
    }

    [Fact]
    public void _09_CN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[9]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[9]), net);
    }

    [Fact]
    public void _09_CN_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[9]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[9]), net, new FilterDataComparer());
    }

    [Fact]
    public void _10_JP_SerializeWithUtf8()
    {
        var utf8 = Utf8Serializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[10]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[10]), utf8);
    }

    [Fact]
    public void _10_JP_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<FilterData>(Filters.GetValueOrDefault(Culture[10]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[10]), net);
    }

    [Fact]
    public void _10_JP_DeserializeWithNET()
    {
        var net = NETSerializer.Deserialize<FilterData>(Jsons.GetValueOrDefault(Culture[10]));
        Assert.Equal(Filters.GetValueOrDefault(Culture[10]), net, new FilterDataComparer());
    }
}