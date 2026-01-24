using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FuzzBenchmark
{
    public static string Json { get; private set; }
    public static FilterData Filter { get; private set; }
    public static IEnumerable<string> Entry { get; private set; }
    public static JsonHelper NETSerializer { get; } = new();

    private static readonly int _fuzzCutoff = 94;
    private static readonly string _sourceString = "Spark fires 3 additional Projectiles";
    private static readonly string _expectedString = "Spark fires an additional Projectile";

    public FuzzBenchmark()
    {
        LoadFile();
        Filter = NETSerializer.Deserialize<FilterData>(Json);
        Entry = from result in Filter.Result
                from filter in result.Entries
                select filter.Text;
    }

    public static void LoadFile()
    {
        //string path = Path.GetFullPath("Data\\en\\Filters.json");
        string path = Environment.CurrentDirectory.Replace("Benchmark", "Library");
        path = path.Substring(0, path.IndexOf("bin")) + "Data\\Lang\\en-US\\Filters.json";  // es,fr,br,jp
        if (!File.Exists(path))
        {
            System.Diagnostics.Debug.WriteLine("File not found : " + path);
            return;
        }

        var fs = new FileStream(path, FileMode.Open);
        using (StreamReader reader = new(fs))
        {
            fs = null;
            Json = reader.ReadToEnd();
        }
    }

    private static void WriteConsoleAndThrow(string value)
    {
        int index = Entry.Select((value, i) => new { value, i })
            .FirstOrDefault(x => x.value == _expectedString)?.i ?? -1;

        Console.WriteLine($"----------------------------------");
        Console.WriteLine($"Entry count : {Entry.Count()}");
        Console.WriteLine($"Found       : {value}");
        Console.WriteLine($"Expected    : {_expectedString}");
        Console.WriteLine($"At index    : {index}");
        Console.WriteLine($"----------------------------------");

        if (value.Length is 0 || value != _expectedString)
            throw new Exception("Invalid setup");
    }

    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractOne))]
    public void SetupOriginFuzzProcessExtractOne()
    {
        var value = OriginFuzzProcessExtractOneBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string OriginFuzzProcessExtractOne() => OriginFuzzProcessExtractOneBench();

    private static string OriginFuzzProcessExtractOneBench()
    {
        var res = FuzzySharp.Process.ExtractOne(_sourceString, Entry, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.Value;
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractOne))]
    public void SetupRaffinertFuzzProcessExtractOne()
    {
        var value = RaffinertFuzzProcessExtractOneBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertFuzzProcessExtractOne() => RaffinertFuzzProcessExtractOneBench();

    private static string RaffinertFuzzProcessExtractOneBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractOne(_sourceString, Entry, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.Value;
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractTop))]
    public void SetupOriginFuzzProcessExtractTop()
    {
        var value = OriginFuzzProcessExtractTopBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string OriginFuzzProcessExtractTop() => OriginFuzzProcessExtractTopBench();

    private static string OriginFuzzProcessExtractTopBench()
    {
        var res = FuzzySharp.Process.ExtractTop(_sourceString, Entry, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), limit: 1, cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractTop))]
    public void SetupRaffinertFuzzProcessExtractTop()
    {
        var value = RaffinertFuzzProcessExtractTopBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertFuzzProcessExtractTop() => RaffinertFuzzProcessExtractTopBench();

    private static string RaffinertFuzzProcessExtractTopBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractTop(_sourceString, Entry, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), limit: 1, cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractAll))]
    public void SetupOriginFuzzProcessExtractAll()
    {
        var value = OriginFuzzProcessExtractAllBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string OriginFuzzProcessExtractAll() => OriginFuzzProcessExtractAllBench();

    private static string OriginFuzzProcessExtractAllBench()
    {
        var res = FuzzySharp.Process.ExtractAll(_sourceString, Entry, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractAll))]
    public void SetupRaffinertFuzzProcessExtractAll()
    {
        var value = RaffinertFuzzProcessExtractAllBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertFuzzProcessExtractAll() => RaffinertFuzzProcessExtractAllBench();

    private static string RaffinertFuzzProcessExtractAllBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractAll(_sourceString, Entry, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractSorted))]
    public void SetupOriginFuzzProcessExtractSorted()
    {
        var value = OriginFuzzProcessExtractSortedBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string OriginFuzzProcessExtractSorted() => OriginFuzzProcessExtractSortedBench();

    private static string OriginFuzzProcessExtractSortedBench()
    {
        string strOut = string.Empty;

        var res = FuzzySharp.Process.ExtractSorted(_sourceString, Entry, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractSorted))]
    public void SetupRaffinertFuzzProcessExtractSorted()
    {
        var value = RaffinertFuzzProcessExtractSortedBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertFuzzProcessExtractSorted() => RaffinertFuzzProcessExtractSortedBench();

    private static string RaffinertFuzzProcessExtractSortedBench()
    {
        string strOut = string.Empty;

        var res = Raffinert.FuzzySharp.Process.ExtractSorted(_sourceString, Entry, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        if (res is not null)
        {
            return res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(OriginFuzzWeightedRatioFirstResult))]
    public void SetupOriginFuzzWeightedRatioFirstResult()
    {
        var value = OriginFuzzWeightedRatioFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string OriginFuzzWeightedRatioFirstResult() => OriginFuzzWeightedRatioFirstResultBench();

    private static string OriginFuzzWeightedRatioFirstResultBench()
    {
        string strOut = string.Empty;

        foreach (var entry in Entry)
        {
            if (FuzzySharp.Fuzz.WeightedRatio(_sourceString, entry) >= _fuzzCutoff)
            {
                return entry; // equal "Spark fires an additional Projectile"
            }
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzWeightedRatioFirstResult))]
    public void SetupRaffinertFuzzWeightedRatioFirstResult()
    {
        var value = RaffinertFuzzWeightedRatioFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertFuzzWeightedRatioFirstResult() => RaffinertFuzzWeightedRatioFirstResultBench();

    private static string RaffinertFuzzWeightedRatioFirstResultBench()
    {
        string strOut = string.Empty;

        foreach (var entry in Entry)
        {
            if (FuzzySharp.Fuzz.WeightedRatio(_sourceString, entry) >= _fuzzCutoff)
            {
                return entry; // equal "Spark fires an additional Projectile"
            }
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(FastenshteinDistanceFromFirstResult))]
    public void SetupFastenshteinDistanceFromFirstResult()
    {
        var value = FastenshteinDistanceFromFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string FastenshteinDistanceFromFirstResult() => FastenshteinDistanceFromFirstResultBench();

    private static string FastenshteinDistanceFromFirstResultBench()
    {
        int maxDistance = 5;

        Fastenshtein.Levenshtein lev = new(_sourceString);
        foreach (var item in Entry)
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= maxDistance)
            {
                return item; // Spark fires an additional Projectile
            }
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(RaffinertLevenshteinDistanceFromFirstResult))]
    public void SetupRaffinertLevenshteinDistanceFromFirstResult()
    {
        var value = RaffinertLevenshteinDistanceFromFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertLevenshteinDistanceFromFirstResult() => RaffinertLevenshteinDistanceFromFirstResultBench();

    private static string RaffinertLevenshteinDistanceFromFirstResultBench()
    {
        int maxDistance = 5;

        Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);
        foreach (var item in Entry)
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= maxDistance)
            {
                return item; // Spark fires an additional Projectile
            }
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(FastenshteinDistanceFromFullLoop))]
    public void SetupFastenshteinDistanceFromFullLoop()
    {
        var value = FastenshteinDistanceFromFullLoopBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string FastenshteinDistanceFromFullLoop() => FastenshteinDistanceFromFullLoopBench();

    private static string FastenshteinDistanceFromFullLoopBench()
    {
        var strOut = string.Empty;
        int maxDistance = 5;

        Fastenshtein.Levenshtein lev = new(_sourceString);

        var distance = maxDistance;
        foreach (var item in Entry)
        {
            int levDistance = lev.DistanceFrom(item);
            if (levDistance <= distance)
            {
                strOut = item;
                distance = levDistance - 1;
            }
        }

        return strOut;
    }

    [GlobalSetup(Target = nameof(RaffinertLevenshteinDistanceFromFullLoop))]
    public void SetupRaffinertLevenshteinDistanceFromFullLoop()
    {
        var value = RaffinertLevenshteinDistanceFromFullLoopBench();
        WriteConsoleAndThrow(value);
    }

    [Benchmark]
    public string RaffinertLevenshteinDistanceFromFullLoop() => RaffinertLevenshteinDistanceFromFullLoopBench();

    private static string RaffinertLevenshteinDistanceFromFullLoopBench()
    {
        var strOut = string.Empty;
        int maxDistance = 5;

        Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);

        var distance = maxDistance;
        foreach (var item in Entry)
        {
            int levDistance = lev.DistanceFrom(item);
            if (levDistance <= distance)
            {
                strOut = item;
                distance = levDistance - 1;
            }
        }

        return strOut;
    }
    /*
    [Benchmark]
    public void FastenshteinDistance()
    {
        int levDistance = Fastenshtein.Levenshtein.Distance(_sourceString, _expectedString);
    }

    [Benchmark]
    public void RaffinertLevenshteinDistance()
    {
        int levDistance = Raffinert.FuzzySharp.Levenshtein.Distance(_sourceString, _expectedString);
    }*/
}
