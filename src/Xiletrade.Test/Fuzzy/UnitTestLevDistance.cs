using System;
using Xunit.Abstractions;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestLevDistance(ITestOutputHelper output) : UnitTest(output)
{
    private static readonly int _cultureEnIndex = 0; // "en-US"

    // It's better to not find a mod and have a response based on other mods from the item than having no response from trade APIs.
    private const int LEVENSHTEIN_DISTANCE_DIVIDER = 4; // Actual xiletrade value is 8

    // TODO, add more use cases based on difference between GGG filters and IN-GAME item description mods (strings).
    public static IEnumerable<object[]> XiletradeTestCases()
    {
        yield return new object[]
        {
            "en-US",
            "Spark fires 3 additional Projectiles",
            "Spark fires an additional Projectile"
        };

        yield return new object[]
        {
            "ko-KR",
            "전기불꽃이 투사체 3개 추가 발사",
            "전기불꽃이 투사체 1개 추가 발사"
        };

        yield return new object[]
        {
            "fr-FR",
            "Étincelle tire 3 Projectiles supplémentaires",
            "Étincelle tire un Projectile supplémentaire"
        };

        yield return new object[]
        {
            "es-ES",
            "Chispa dispara 3 Proyectiles adicionales",
            "Chispa dispara un Proyectil adicional"
        };

        yield return new object[]
        {
            "de-DE",
            "'Funken' feuert 3 zusätzliche Projektile",
            "'Funken' feuert ein zusätzliches Projektil"
        };

        // Example that clearly show limits of 'closest' match with lev distance algo usage.
        // That's why manual parser rules are mandatory before using fuzz or lev distance.
        yield return new object[]
        {
            "pt-BR",
            "Fagulha dispara 3 Projéteis adicionais",
            "Centelha atira um Projétil adicional"
        };

        yield return new object[]
        {
            "ru-RU",
            "Искра выпускает дополнительных снарядов: 3",
            "Искра выпускает дополнительный снаряд"
        };

        yield return new object[]
        {
            "th-TH",
            "ประกายสายฟ้า (Spark) ยิงโพรเจกไทล์ เพิ่มเติม 3 ลูก",
            "ประกายสายฟ้า (Spark) ยิงโพรเจกไทล์ เพิ่มเติม 1 ลูก"
        };

        // Result : 法術發射 1 個額外投射物
        // Another concrete example that show limits of closest match.
        yield return new object[]
        {
            "zh-TW",
            "電球發射 3 個額外投射物",
            "電球發射一個額外投射物"
        };

        yield return new object[]
        {
            "ja-JP",
            "スパークは投射物を追加で3個放つ",
            "スパークは投射物を追加で1個放つ"
        };
    }

    public static IEnumerable<object[]> TestCultures()
    {
        foreach (var tc in XiletradeTestCases())
        {
            yield return new object[] { tc[0] };
        }
    }

    private static int GetMaxDistance(ReadOnlySpan<char> mod)
    {
        int maxDistance = mod.Length / LEVENSHTEIN_DISTANCE_DIVIDER;
        if (maxDistance is 0)
        {
            maxDistance = 1;
        }
        return maxDistance;
    }

    [Theory]
    [Trait("Type", "Exploratory")]
    [MemberData(nameof(TestCultures))]
    public void _00_Base_List_Count(string culture)
    {
        var idxCulture = GetCultureIndex(culture);
        DisplayListCount(idxCulture);

        Assert.True(idxCulture >= 0);
    }

    #region Levenshtein.Distance
    [Fact]
    [Trait("Type", "Exploratory")]
    public void _01_Fastenshtein_Distance_ParserRules()
    {
        List<(int distance, string source, string expected)> listDistance = new();

        foreach (var (oldMod, newMod) in AllParserMods[_cultureEnIndex])
        {
            int levenshteinDistance = Fastenshtein.Levenshtein.Distance(oldMod, newMod);
            listDistance.Add((levenshteinDistance, oldMod, newMod));
        }
        DisplayDistanceList(listDistance);

        Assert.True(true);
    }

    [Fact]
    [Trait("Type", "Exploratory")]
    public void _01_Raffinert_Levenshtein_Distance_ParserRules()
    {
        List<(int distance, string source, string expected)> listDistance = new();

        foreach (var (oldMod, newMod) in AllParserMods[_cultureEnIndex])
        {
            int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(oldMod, newMod);
            listDistance.Add((levenshteinDistance, oldMod, newMod));
        }
        DisplayDistanceList(listDistance);

        Assert.True(true);
    }

    [Theory]
    [Trait("Type", "Exploratory")]
    [MemberData(nameof(XiletradeTestCases))]
    public void _02_Fastenshtein_Distance(string culture, string source, string expected)
    {
        int levenshteinDistance = Fastenshtein.Levenshtein.Distance(source, expected);
        DisplayDistance(levenshteinDistance, source, expected);

        Assert.True(true);
    }

    [Theory]
    [Trait("Type", "Exploratory")]
    [MemberData(nameof(XiletradeTestCases))]
    public void _02_Raffinert_Levenshtein_Distance(string culture, string source, string expected)
    {
        int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(source, expected);
        DisplayDistance(levenshteinDistance, source, expected);

        Assert.True(true);
    }
    #endregion

    #region Levenshtein.DistanceFrom
    /*
    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _03_Fastenshtein_DistanceFrom_FirstResult(string culture, string source, string expected)
    {
        string result = string.Empty;
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Fastenshtein.Levenshtein lev = new(source);

        foreach (var item in AllFilterEntries[idxCulture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= levMaxDistance)
            {
                result = item;
                break;
            }
        }
        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _03_Raffinert_Levenshtein_DistanceFrom_FirstResult(string culture, string source, string expected)
    {
        string result = string.Empty;
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Raffinert.FuzzySharp.Levenshtein lev = new(source);

        foreach (var item in AllFilterEntries[idxCulture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= levMaxDistance)
            {
                result = item;
                break;
            }
        }
        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _04_Fastenshtein_DistanceFrom_FullLoop(string culture, string source, string expected)
    {
        var result = string.Empty;
        Dictionary<string, int> dict = new();
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Fastenshtein.Levenshtein lev = new(source);
        
        foreach (var item in AllFilterEntries[idxCulture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= levMaxDistance)
            {
                dict.TryAdd(item, levenshteinDistance);
            }
        }
        result = dict.Count > 0 ? dict.MinBy(kvp => kvp.Value).Key : string.Empty;
        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _04_Raffinert_Levenshtein_DistanceFrom_FullLoop(string culture, string source, string expected)
    {
        var result = string.Empty;
        Dictionary<string, int> dict = new();
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Raffinert.FuzzySharp.Levenshtein lev = new(source);
        
        foreach (var item in AllFilterEntries[idxCulture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= levMaxDistance)
            {
                dict.TryAdd(item, levenshteinDistance);
            }
        }
        result = dict.Count > 0 ? dict.MinBy(kvp => kvp.Value).Key : string.Empty;
        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }
    */
    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _05_Fastenshtein_DistanceFrom_Closest(string culture, string source, string expected)
    {
        string result = string.Empty;
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Fastenshtein.Levenshtein lev = new(source);

        var closestMatch = AllFilterEntries[idxCulture]
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item) })
                .Where(x => x.Distance <= levMaxDistance)
                .MinBy(x => x.Distance);
        if (closestMatch is not null)
        {
            result = closestMatch.Item;
        }

        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _05_Raffinert_Levenshtein_DistanceFrom_Closest(string culture, string source, string expected)
    {
        string result = string.Empty;
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);
        Raffinert.FuzzySharp.Levenshtein lev = new(source);

        var closestMatch = AllFilterEntries[idxCulture]
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item) })
                .Where(x => x.Distance <= levMaxDistance)
                .MinBy(x => x.Distance);
        if (closestMatch is not null)
        {
            result = closestMatch.Item;
        }

        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _06_Raffinert_Levenshtein_DistanceFrom_Closest_NoLinq(string culture, string source, string expected)
    {
        string result = string.Empty;
        var bestDistance = int.MaxValue;
        var idxCulture = GetCultureIndex(culture);
        var levMaxDistance = GetMaxDistance(source);

        using Raffinert.FuzzySharp.Levenshtein lev = new(source);

        foreach (var item in AllFilterEntries[idxCulture])
        {
            var distance = lev.DistanceFrom(item);

            if (distance > levMaxDistance)
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                result = item;

                if (distance is 0)
                    break;
            }
        }

        DisplayResult(culture, source, result, expected, levMaxDistance);

        Assert.True(idxCulture >= 0);
        Assert.Equal(result, expected);
    }
    #endregion
}
