using FuzzySharp.SimilarityRatio.Scorer.Composite;
using FuzzySharp.SimilarityRatio;
using FuzzySharp;
using System.Text;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestParserEn : UnitTest
{
    public UnitTestParserEn() : base(0)
    {
        
    }

    [Fact]
    public void _00_EN_Fuzz() // algo test
    {
        bool test = true;
        List<int> listWeightedRatioScorerProcess = new();
        List<int> listWeightedRatioScorerFuzz = new();
        foreach (var mod in ParserDat.Mods)
        {
            if (mod.Replace is "equals")
            {
                var result = Process.ExtractOne(mod.Old, [mod.New], (s) => s, ScorerCache.Get<WeightedRatioScorer>(), cutoff: 60);
                if (result is null)
                {
                    test = false;
                    break;
                }
                listWeightedRatioScorerProcess.Add(result.Score);

                listWeightedRatioScorerFuzz.Add(Fuzz.WeightedRatio(mod.Old, mod.New, FuzzySharp.PreProcess.PreprocessMode.Full));
            }
        }
        var str = "WeightedRatioScorerProcess : " + string.Join(", ", listWeightedRatioScorerProcess) + "\n"
            + "WeightedRatioScorerFuzz    : " + string.Join(", ", listWeightedRatioScorerFuzz);

        Assert.True(test);
    }

    [Fact]
    public void _01_EN_Fastenshtein() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();
        foreach (var mod in ParserDat.Mods)
        {
            if (mod.Replace is "equals")
            {
                int levenshteinDistance = Fastenshtein.Levenshtein.Distance(mod.Old, mod.New);
                listScore.Add(levenshteinDistance);
                sb.Append($"Old  : {mod.Old}");
                sb.AppendLine();
                sb.Append($"New  : {mod.New}");
                sb.AppendLine();
                sb.Append($"Score: {levenshteinDistance}");
                sb.AppendLine();
                sb.AppendLine();
            }
        }
        var str = "Fastenshtein : " + string.Join(", ", listScore);
        var str2 = sb.ToString();

        Assert.True(test);
    }

    [Fact]
    public void _02_EN_FastenshteinSingle() // algo test
    {
        bool test = true;
        List<int> listScore = new();
        StringBuilder sb = new();
        string oldMod = "Spark fires 3 additional Projectiles";
        string newMod = "Spark fires an additional Projectile";
        int levenshteinDistance = Fastenshtein.Levenshtein.Distance(oldMod, newMod);
        listScore.Add(levenshteinDistance);
        sb.Append($"Old  : {oldMod}");
        sb.AppendLine();
        sb.Append($"New  : {newMod}");
        sb.AppendLine();
        sb.Append($"Score: {levenshteinDistance}");
        sb.AppendLine();
        sb.AppendLine();
        var str2 = sb.ToString();

        /*  Old  : Spark fires 3 additional Projectiles
            New  : Spark fires an additional Projectile
            Score: 3
        */

        Assert.True(test);
    }

    [Fact]
    public void _03_EN_Fastenshtein() // algo test
    {
        bool test = true;
        int maxDistance = 5;
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            Fastenshtein.Levenshtein lev = new(str);
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    strOut = item; // Spark fires an additional Projectile
                    break;
                }
            }
        }

        Assert.True(test);
    }

    [Fact]
    public void _04_EN_FastenshteinFullLoop() // algo test
    {
        bool test = true;

        int maxDistance = 5;
        string str = "Spark fires 3 additional Projectiles";
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            Fastenshtein.Levenshtein lev = new(str);
            Dictionary<string, int> dict = new();
            foreach (var item in entrySeek)
            {
                int levenshteinDistance = lev.DistanceFrom(item);
                if (levenshteinDistance <= maxDistance)
                {
                    dict.TryAdd(item, levenshteinDistance);
                }
            }
            var strOut = dict.MinBy(kvp => kvp.Value).Key; // Spark fires an additional Projectile
        }

        Assert.True(test);
    }
}
