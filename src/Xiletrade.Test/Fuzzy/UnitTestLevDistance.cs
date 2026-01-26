using Xunit.Abstractions;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestLevDistance(ITestOutputHelper output) : UnitTest(output)
{
    private static readonly int _cultureIndex = 0; // "en-US"

    private readonly int _levMaxDistance = 5;

    public static IEnumerable<object[]> XiletradeTestCases()
    {
        // "en-US"
        yield return new object[]
        {
            0,
            "Spark fires 3 additional Projectiles",
            "Spark fires an additional Projectile"
        };

        // "ko-KR" : TOREDO bad example
        yield return new object[]
        {
            1,
            "전기불꽃이 투사체 3개 추가 발사",
            "전기불꽃이 투사체 1개 추가 발사"
        };

        // "fr-FR"
        yield return new object[]
        {
            2,
            "Étincelle tire 3 Projectiles supplémentaires",
            "Étincelle tire un Projectile supplémentaire"
        };

        // "es-ES"
        yield return new object[]
        {
            3,
            "Chispa dispara 3 Proyectiles adicionales",
            "Chispa dispara un Proyectil adicional"
        };

        // "de-DE"
        yield return new object[]
        {
            4,
            "'Funken' feuert 3 zusätzliche Projektile",
            "'Funken' feuert ein zusätzliches Projektil"
        };

        // "pt-BR"
        yield return new object[]
        {
            5,
            "Fagulha dispara 3 Projéteis adicionais",
            "Centelha atira um Projétil adicional"
        };

        // "ru-RU"
        yield return new object[]
        {
            6,
            "Искра выпускает дополнительных снарядов: 3",
            "Искра выпускает дополнительный снаряд"
        };

        // "th-TH" : TOREDO bad example
        yield return new object[]
        {
            7,
            "ประกายสายฟ้า (Spark) ยิงโพรเจกไทล์ เพิ่มเติม 3 ลูก",
            "ประกายสายฟ้า (Spark) ยิงโพรเจกไทล์ เพิ่มเติม 1 ลูก"
        };

        // "zh-TW" : TOREDO bad example
        yield return new object[]
        {
            8,
            "電球發射一個額外投射物",
            "電球發射一個額外投射物"
        };

        // "ja-JP" : TOREDO bad example
        yield return new object[]
        {
            9,
            "スパークは投射物を追加で3個放つ",
            "スパークは投射物を追加で1個放つ"
        };
    }
    #region Levenshtein.Distance
    [Fact]
    public void _01_Fastenshtein_Distance_ParserRules()
    {
        List<(int distance, string source, string expected)> listDistance = new();

        foreach (var (oldMod, newMod) in AllParserMods[_cultureIndex])
        {
            int levenshteinDistance = Fastenshtein.Levenshtein.Distance(oldMod, newMod);
            listDistance.Add((levenshteinDistance, oldMod, newMod));
        }
        DisplayDistanceList(listDistance);

        Assert.True(true);
    }

    [Fact]
    public void _01_Raffinert_Levenshtein_Distance_ParserRules()
    {
        List<(int distance, string source, string expected)> listDistance = new();

        foreach (var (oldMod, newMod) in AllParserMods[_cultureIndex])
        {
            int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(oldMod, newMod);
            listDistance.Add((levenshteinDistance, oldMod, newMod));
        }
        DisplayDistanceList(listDistance);

        Assert.True(true);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _02_Fastenshtein_Distance(int culture, string source, string expected)
    {
        int levenshteinDistance = Fastenshtein.Levenshtein.Distance(source, expected);
        DisplayDistance(levenshteinDistance, source, expected);

        Assert.True(true);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _02_Raffinert_Levenshtein_Distance(int culture, string source, string expected)
    {
        int levenshteinDistance = Raffinert.FuzzySharp.Levenshtein.Distance(source, expected);
        DisplayDistance(levenshteinDistance, source, expected);

        Assert.True(true);
    }
    #endregion

    #region Levenshtein.DistanceFrom
    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _03_Fastenshtein_DistanceFrom_FirstResult(int culture, string source, string expected)
    {
        string result = string.Empty;
        Fastenshtein.Levenshtein lev = new(source);

        foreach (var item in AllFilterEntries[culture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
            {
                result = item;
                break;
            }
        }
        DisplayResult(culture, result, expected);

        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _03_Raffinert_Levenshtein_DistanceFrom_FirstResult(int culture, string source, string expected)
    {
        string result = string.Empty;
        Raffinert.FuzzySharp.Levenshtein lev = new(source);

        foreach (var item in AllFilterEntries[culture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
            {
                result = item;
                break;
            }
        }
        DisplayResult(culture, result, expected);

        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _04_Fastenshtein_DistanceFrom_FullLoop(int culture, string source, string expected)
    {
        var result = string.Empty;
        Dictionary<string, int> dict = new();
        Fastenshtein.Levenshtein lev = new(source);
        
        foreach (var item in AllFilterEntries[culture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
            {
                dict.TryAdd(item, levenshteinDistance);
            }
        }
        result = dict.MinBy(kvp => kvp.Value).Key;
        DisplayResult(culture, result, expected);

        Assert.Equal(result, expected);
    }

    [Theory]
    [MemberData(nameof(XiletradeTestCases))]
    public void _04_Raffinert_Levenshtein_DistanceFrom_FullLoop(int culture, string source, string expected)
    {
        var result = string.Empty;
        Dictionary<string, int> dict = new();
        Raffinert.FuzzySharp.Levenshtein lev = new(source);
        
        foreach (var item in AllFilterEntries[culture])
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
            {
                dict.TryAdd(item, levenshteinDistance);
            }
        }
        result = dict.MinBy(kvp => kvp.Value).Key;
        DisplayResult(culture, result, expected);

        Assert.Equal(result, expected);
    }
    #endregion
}
