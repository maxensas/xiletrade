using FluentAssertions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Shared;

namespace Xiletrade.Test.CI.Parsing;

/// <summary>
/// <see cref="FilterResultExtensions.IsModMatch"/> must give the same result as the regex previously built by ModFilter.
/// </summary>
public class ModMatchTests
{
    private const int SAMPLE_BY_FILTER = 25;

    [Theory]
    // signs
    [InlineData("+5 to maximum Life", "+# to maximum Life")]
    [InlineData("-5 to maximum Life", "+# to maximum Life")]
    [InlineData("+# to x", "# to x")]
    // several numbers / decimals
    [InlineData("Adds 12 to 24 Physical Damage", "Adds # to # Physical Damage")]
    [InlineData("0.5% of Life Regenerated per second", "#% of Life Regenerated per second")]
    [InlineData("1.5", "1.5.3")]
    [InlineData("5-3", "53")]
    // case / mismatch
    [InlineData("5% INCREASED Damage", "#% increased Damage")]
    [InlineData("5% increased Damage", "5% increased Attack Speed")]
    // regex special characters (literal text)
    [InlineData("Grants Level 20 Aura (Hatred)", "Grants Level # Aura (Hatred)")]
    [InlineData("a.b", "axb")]
    // lines / end of text
    [InlineData("Adds 1 to 2 Fire Damage", "Adds # to # Fire Damage\n")]
    [InlineData("5 to x\n6 to y", "# to x")]
    // languages
    [InlineData("+5 À LA RÉSISTANCE AU FEU", "+# à la résistance au feu")]
    [InlineData("+5 к сопротивлению огню", "+# К СОПРОТИВЛЕНИЮ ОГНЮ")]
    [InlineData("火耐性 ５%", "火耐性 #%")]
    // empty
    [InlineData("", "")]
    [InlineData("x", "")]
    public void _01_Should_Match_As_The_Previous_Regex(string mod, string text)
    {
        FilterResultExtensions.IsModMatch(mod, text).Should().Be(GetPreviousRegex(mod).IsMatch(text));
    }

    [Theory]
    [InlineData("en-US", "Filters")]
    [InlineData("fr-FR", "Filters")]
    [InlineData("de-DE", "Filters")]
    [InlineData("es-ES", "Filters")]
    [InlineData("pt-BR", "Filters")]
    [InlineData("ru-RU", "Filters")]
    [InlineData("ja-JP", "Filters")]
    [InlineData("ko-KR", "Filters")]
    [InlineData("th-TH", "Filters")]
    [InlineData("zh-CN", "Filters")]
    [InlineData("zh-TW", "Filters")]
    [InlineData("en-US", "FiltersTwo")]
    [InlineData("fr-FR", "FiltersTwo")]
    [InlineData("de-DE", "FiltersTwo")]
    [InlineData("es-ES", "FiltersTwo")]
    [InlineData("pt-BR", "FiltersTwo")]
    [InlineData("ru-RU", "FiltersTwo")]
    [InlineData("ja-JP", "FiltersTwo")]
    [InlineData("ko-KR", "FiltersTwo")]
    [InlineData("th-TH", "FiltersTwo")]
    [InlineData("zh-CN", "FiltersTwo")]
    [InlineData("zh-TW", "FiltersTwo")]
    public void _02_Should_Match_As_The_Previous_Regex_On_Filters(string lang, string fileName)
    {
        using var stream = File.OpenRead(GetDataPath(lang, fileName));
        using var doc = JsonDocument.Parse(stream);
        var random = new Random(1234);
        int positives = 0;

        foreach (var filter in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            var texts = filter.GetProperty("entries").EnumerateArray()
                .Select(e => e.GetProperty("text").GetString() ?? string.Empty).ToArray();
            int sample = Math.Min(SAMPLE_BY_FILTER, texts.Length);
            for (int s = 0; s < sample; s++)
            {
                var mod = GetMod(texts[random.Next(texts.Length)], random);
                var regex = GetPreviousRegex(mod);
                foreach (var text in texts)
                {
                    bool expected = regex.IsMatch(text);
                    if (FilterResultExtensions.IsModMatch(mod, text) != expected)
                    {
                        Assert.Fail($"mod '{mod}' / text '{text}' : expected {expected}");
                    }
                    if (expected)
                    {
                        positives++;
                    }
                }
            }
        }
        positives.Should().BeGreaterThan(0);
    }

    // previous implementation : ModFilter.GetInputRegex
    private static Regex GetPreviousRegex(string mod)
    {
        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(mod, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);
        return new Regex("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);
    }

    // item modifier built from a filter entry : numbers, raw text, case change, mutation or suffix
    private static string GetMod(string text, Random random)
    {
        var mod = new StringBuilder(text.Length + 8);
        foreach (char c in text)
        {
            if (c is '#')
            {
                mod.Append(GetNumber(random));
                continue;
            }
            mod.Append(c);
        }
        var kind = random.Next(10);
        if (kind is 6)
        {
            return text;
        }
        if (kind is 7)
        {
            return mod.ToString().ToUpperInvariant();
        }
        if (kind is 8 && mod.Length > 0)
        {
            mod[random.Next(mod.Length)] = "x1.+-# \n"[random.Next(8)];
        }
        if (kind is 9)
        {
            mod.Append(new[] { "", "\n", " ", "1" }[random.Next(4)]);
        }
        return mod.ToString();
    }

    private static string GetNumber(Random random)
    {
        var kind = random.Next(20);
        return kind < 10 ? random.Next(0, 301).ToString()
            : kind < 15 ? random.Next(0, 100) + "." + random.Next(0, 100)
            : kind < 18 ? "-" + random.Next(1, 100)
            : "+" + random.Next(1, 100);
    }

    private static string GetDataPath(string lang, string fileName)
    {
        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = basePath[..basePath.IndexOf("bin")];
        return Path.Combine(basePath, "Data", "Lang", lang, fileName + ".json");
    }
}
