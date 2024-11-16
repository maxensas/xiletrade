using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Test;

/// <summary>
/// Unit test used to show incorrect Serialization made by System.Text.Json (.NET8) compared to Utf8Json.
/// </summary>
/// <remarks>
/// Will evolve with futur tests / unicode configuration.
/// </remarks>
public class UnitTestCurrency : UnitTest<CurrencyResult>
{
    public UnitTestCurrency() : base("Currency")
    {

    }

    [Fact]
    public void _00_EN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[0]));

        Assert.Equal(Jsons.GetValueOrDefault(Culture[0]), net);
    }

    [Fact]
    public void _01_KR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[1]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[1]), net);
    }

    [Fact]
    public void _02_FR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[2]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[2]), net);
    }

    [Fact]
    public void _03_ES_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[3]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[3]), net);
    }

    [Fact]
    public void _04_DE_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[4]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[4]), net);
    }

    [Fact]
    public void _05_BR_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[5]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[5]), net);
    }

    [Fact]
    public void _06_RU_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[6]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[6]), net);
    }
    [Fact]
    public void _07_TH_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[7]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[7]), net);
    }
    [Fact]
    public void _08_TW_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[8]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[8]), net);
    }
    [Fact]
    public void _09_CN_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[9]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[9]), net);
    }
    [Fact]
    public void _10_JP_SerializeWithNET()
    {
        var net = NETSerializer.Serialize<CurrencyResult>(Serializable.GetValueOrDefault(Culture[10]));
        Assert.Equal(Jsons.GetValueOrDefault(Culture[10]), net);
    }
}