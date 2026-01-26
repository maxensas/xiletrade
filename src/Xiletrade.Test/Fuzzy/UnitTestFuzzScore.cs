using Xunit.Abstractions;

namespace Xiletrade.Test.Fuzzy;

public class UnitTestFuzzScore : UnitTest
{
    private readonly ITestOutputHelper _output;

    private static readonly int _fuzzCutoff = 60;
    private static readonly int _cultureIndex = 0; // "en-US"

    public UnitTestFuzzScore(ITestOutputHelper output) : base(null)
    {
        _output = output;
    }

    [Fact]
    public void _01_ParserRules_Original_Fuzz_WeightedRatio()
    {
        bool test = true;
        List<int> listWeightedRatioScorerProcess = new();
        List<int> listWeightedRatioScorerFuzz = new();
        List<(string, string)> listMods = new();
        foreach (var (oldMod, newMod) in AllParserMods[_cultureIndex])
        {
            var result = FuzzySharp.Process.ExtractOne(oldMod, [newMod], static s => s,
                    FuzzySharp.SimilarityRatio.ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
            if (result is null)
            {
                test = false;
                break;
            }
            listWeightedRatioScorerProcess.Add(result.Score);
            listMods.Add((oldMod, newMod));
            listWeightedRatioScorerFuzz.Add(FuzzySharp.Fuzz.WeightedRatio(oldMod, newMod, FuzzySharp.PreProcess.PreprocessMode.Full));
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("Cutoff: " + _fuzzCutoff);
        _output.WriteLine(string.Empty);
        _output.WriteLine("WeightedRatioScorerProcess :");
        _output.WriteLine(string.Join(", ", listWeightedRatioScorerProcess));
        _output.WriteLine("Sum: " + listWeightedRatioScorerProcess.Sum());
        _output.WriteLine(string.Empty);
        _output.WriteLine("WeightedRatioScorerFuzz :");
        _output.WriteLine(string.Join(", ", listWeightedRatioScorerFuzz));
        _output.WriteLine("Sum: " + listWeightedRatioScorerFuzz.Sum());
        _output.WriteLine("-------------------------------------");
        _output.WriteLine(string.Empty);

        for (int i = 0; i < listMods.Count; i++)
        {
            _output.WriteLine("input1: "+ listMods[i].Item1);
            _output.WriteLine("input2: "+ listMods[i].Item2);
            _output.WriteLine("WeightedRatioScorerProcess: " + listWeightedRatioScorerProcess[i]);
            _output.WriteLine("WeightedRatioScorerFuzz: " + listWeightedRatioScorerFuzz[i]);
            _output.WriteLine(string.Empty);
        }
        _output.WriteLine(string.Empty);

        /* Diff
         * 
        input1: Your Maps have #% chance to contain The Sacred Grove
        input2: Area contains The Sacred Grove
        WeightedRatioScorerProcess: 79
        WeightedRatioScorerFuzz: 79
        */

        Assert.True(test);
    }

    [Fact]
    public void _02_ParserRules_Raffinert_Fuzz_WeightedRatio()
    {
        bool test = true;
        List<int> listWeightedRatioScorerProcess = new();
        List<int> listWeightedRatioScorerFuzz = new();
        List<(string, string)> listMods = new();
        foreach (var (oldMod, newMod) in AllParserMods[_cultureIndex])
        {
            var result = Raffinert.FuzzySharp.Process.ExtractOne(oldMod, [newMod], static s => s,
                    Raffinert.FuzzySharp.SimilarityRatio.ScorerCache.Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
            if (result is null)
            {
                test = false;
                break;
            }
            listWeightedRatioScorerProcess.Add(result.Score);
            listMods.Add((oldMod, newMod));
            listWeightedRatioScorerFuzz.Add(Raffinert.FuzzySharp.Fuzz.WeightedRatio(oldMod, newMod, Raffinert.FuzzySharp.PreProcess.PreprocessMode.Full));
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("Cutoff: " + _fuzzCutoff);
        _output.WriteLine(string.Empty);
        _output.WriteLine("WeightedRatioScorerProcess :");
        _output.WriteLine(string.Join(", ", listWeightedRatioScorerProcess));
        _output.WriteLine("Sum: " + listWeightedRatioScorerProcess.Sum());
        _output.WriteLine(string.Empty);
        _output.WriteLine("WeightedRatioScorerFuzz :");
        _output.WriteLine(string.Join(", ", listWeightedRatioScorerFuzz));
        _output.WriteLine("Sum: " + listWeightedRatioScorerFuzz.Sum());
        _output.WriteLine("-------------------------------------");
        _output.WriteLine(string.Empty);

        for (int i = 0; i < listMods.Count; i++)
        {
            _output.WriteLine("input1: " + listMods[i].Item1);
            _output.WriteLine("input2: " + listMods[i].Item2);
            _output.WriteLine("WeightedRatioScorerProcess: " + listWeightedRatioScorerProcess[i]);
            _output.WriteLine("WeightedRatioScorerFuzz: " + listWeightedRatioScorerFuzz[i]);
            _output.WriteLine(string.Empty);
        }
        _output.WriteLine(string.Empty);

        /* Diff
         * 
        input1: Your Maps have #% chance to contain The Sacred Grove
        input2: Area contains The Sacred Grove
        WeightedRatioScorerProcess: 82
        WeightedRatioScorerFuzz: 82
        */

        Assert.True(test);
    }
}
