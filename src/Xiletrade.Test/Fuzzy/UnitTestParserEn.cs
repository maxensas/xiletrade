using FuzzySharp.SimilarityRatio.Scorer.Composite;
using FuzzySharp.SimilarityRatio;
using FuzzySharp;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestParserEn : UnitTest
{
    public UnitTestParserEn() : base(0)
    {
        
    }

    [Fact]
    public void _00_EN_ExtractOne_WeightedRatio()
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
}
