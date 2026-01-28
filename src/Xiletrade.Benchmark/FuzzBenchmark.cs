using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
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
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[RankColumn]
public class FuzzBenchmark
{
    public static IEnumerable<string> FilterEntries { get; private set; }

    private static readonly int _fuzzCutoff = 94;
    private static readonly int _levMaxDistance = 5;
    private static readonly string _sourceString = "Spark fires 3 additional Projectiles";
    private static readonly string _expectedString = "Spark fires an additional Projectile";

    public FuzzBenchmark()
    {
        var json = LoadFile();
        if (json.Length is 0)
        {
            return;
        }
        JsonHelper serializer = new();
        var filterData = serializer.Deserialize<FilterData>(json);
        FilterEntries = from result in filterData.Result
                from filter in result.Entries
                select filter.Text;
    }

    public static string LoadFile()
    {
        string path = Environment.CurrentDirectory.Replace("Benchmark", "Library");
        path = string.Concat(path.AsSpan(0, path.IndexOf("bin")), "Data\\Lang\\en-US\\Filters.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found : " + path);
        }

        var fs = new FileStream(path, FileMode.Open);
        using StreamReader reader = new(fs);
        fs = null;
        return reader.ReadToEnd();
    }

    private static void WriteConsoleAndThrow(string value)
    {
        int index = FilterEntries.Select((value, i) => new { value, i })
            .FirstOrDefault(x => x.value == _expectedString)?.i ?? -1;

        Console.WriteLine($"----------------------------------");
        Console.WriteLine($"Entry count : {FilterEntries.Count()}");
        Console.WriteLine($"Found       : {value}");
        Console.WriteLine($"Expected    : {_expectedString}");
        Console.WriteLine($"At index    : {index}");
        Console.WriteLine($"----------------------------------");

        /*
        Entry count : 14925
        Found       : Spark fires an additional Projectile
        Expected    : Spark fires an additional Projectile
        At index    : 10237 
        */

        if (value.Length is 0 || value != _expectedString)
            throw new Exception("Invalid setup");
    }
    /*
    #region ProcessExtractOne 
    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractOne))]
    public void SetupOriginFuzzProcessExtractOne()
    {
        var value = OriginFuzzProcessExtractOneBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractOne")]
    [Benchmark]
    public string OriginFuzzProcessExtractOne() => OriginFuzzProcessExtractOneBench();

    private static string OriginFuzzProcessExtractOneBench()
    {
        var res = FuzzySharp.Process.ExtractOne(_sourceString, FilterEntries, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.Value;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractOne))]
    public void SetupRaffinertFuzzProcessExtractOne()
    {
        var value = RaffinertFuzzProcessExtractOneBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractOne")]
    [Benchmark]
    public string RaffinertFuzzProcessExtractOne() => RaffinertFuzzProcessExtractOneBench();

    private static string RaffinertFuzzProcessExtractOneBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractOne(_sourceString, FilterEntries, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);
        
        return res is null ? string.Empty : res.Value;
    }
    #endregion

    #region ProcessExtractTop
    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractTop))]
    public void SetupOriginFuzzProcessExtractTop()
    {
        var value = OriginFuzzProcessExtractTopBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractTop")]
    [Benchmark]
    public string OriginFuzzProcessExtractTop() => OriginFuzzProcessExtractTopBench();

    private static string OriginFuzzProcessExtractTopBench()
    {
        var res = FuzzySharp.Process.ExtractTop(_sourceString, FilterEntries, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), limit: 1, cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractTop))]
    public void SetupRaffinertFuzzProcessExtractTop()
    {
        var value = RaffinertFuzzProcessExtractTopBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractTop")]
    [Benchmark]
    public string RaffinertFuzzProcessExtractTop() => RaffinertFuzzProcessExtractTopBench();

    private static string RaffinertFuzzProcessExtractTopBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractTop(_sourceString, FilterEntries, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), limit: 1, cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }
    #endregion

    #region ProcessExtractAll
    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractAll))]
    public void SetupOriginFuzzProcessExtractAll()
    {
        var value = OriginFuzzProcessExtractAllBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractAll")]
    [Benchmark]
    public string OriginFuzzProcessExtractAll() => OriginFuzzProcessExtractAllBench();

    private static string OriginFuzzProcessExtractAllBench()
    {
        var res = FuzzySharp.Process.ExtractAll(_sourceString, FilterEntries, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractAll))]
    public void SetupRaffinertFuzzProcessExtractAll()
    {
        var value = RaffinertFuzzProcessExtractAllBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractAll")]
    [Benchmark]
    public string RaffinertFuzzProcessExtractAll() => RaffinertFuzzProcessExtractAllBench();

    private static string RaffinertFuzzProcessExtractAllBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractAll(_sourceString, FilterEntries, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }
    #endregion

    #region ProcessExtractSorted
    [GlobalSetup(Target = nameof(OriginFuzzProcessExtractSorted))]
    public void SetupOriginFuzzProcessExtractSorted()
    {
        var value = OriginFuzzProcessExtractSortedBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractSorted")]
    [Benchmark]
    public string OriginFuzzProcessExtractSorted() => OriginFuzzProcessExtractSortedBench();

    private static string OriginFuzzProcessExtractSortedBench()
    {
        var res = FuzzySharp.Process.ExtractSorted(_sourceString, FilterEntries, (s) => s,
            FuzzySharp.SimilarityRatio.ScorerCache
            .Get<FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }

    [GlobalSetup(Target = nameof(RaffinertFuzzProcessExtractSorted))]
    public void SetupRaffinertFuzzProcessExtractSorted()
    {
        var value = RaffinertFuzzProcessExtractSortedBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("ProcessExtractSorted")]
    [Benchmark]
    public string RaffinertFuzzProcessExtractSorted() => RaffinertFuzzProcessExtractSortedBench();

    private static string RaffinertFuzzProcessExtractSortedBench()
    {
        var res = Raffinert.FuzzySharp.Process.ExtractSorted(_sourceString, FilterEntries, (s) => s,
            Raffinert.FuzzySharp.SimilarityRatio.ScorerCache
            .Get<Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite.WeightedRatioScorer>(), cutoff: _fuzzCutoff);

        return res is null ? string.Empty : res.FirstOrDefault()?.Value;
    }
    #endregion

    #region WeightedRatio
    [GlobalSetup(Target = nameof(OriginFuzzWeightedRatioFirstResult))]
    public void SetupOriginFuzzWeightedRatioFirstResult()
    {
        var value = OriginFuzzWeightedRatioFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("WeightedRatio")]
    [Benchmark]
    public string OriginFuzzWeightedRatioFirstResult() => OriginFuzzWeightedRatioFirstResultBench();

    private static string OriginFuzzWeightedRatioFirstResultBench()
    {
        foreach (var entry in FilterEntries)
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

    [BenchmarkCategory("WeightedRatio")]
    [Benchmark]
    public string RaffinertFuzzWeightedRatioFirstResult() => RaffinertFuzzWeightedRatioFirstResultBench();

    private static string RaffinertFuzzWeightedRatioFirstResultBench()
    {
        foreach (var entry in FilterEntries)
        {
            if (FuzzySharp.Fuzz.WeightedRatio(_sourceString, entry) >= _fuzzCutoff)
            {
                return entry; // equal "Spark fires an additional Projectile"
            }
        }

        return string.Empty;
    }
    #endregion

    #region Levenshtein.DistanceFrom
    [GlobalSetup(Target = nameof(FastenshteinDistanceFromFirstResult))]
    public void SetupFastenshteinDistanceFromFirstResult()
    {
        var value = FastenshteinDistanceFromFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public string FastenshteinDistanceFromFirstResult() => FastenshteinDistanceFromFirstResultBench();

    private static string FastenshteinDistanceFromFirstResultBench()
    {
        Fastenshtein.Levenshtein lev = new(_sourceString);
        foreach (var item in FilterEntries)
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
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

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public string RaffinertLevenshteinDistanceFromFirstResult() => RaffinertLevenshteinDistanceFromFirstResultBench();

    private static string RaffinertLevenshteinDistanceFromFirstResultBench()
    {
        using Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);
        foreach (var item in FilterEntries)
        {
            int levenshteinDistance = lev.DistanceFrom(item);
            if (levenshteinDistance <= _levMaxDistance)
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

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public string FastenshteinDistanceFromFullLoop() => FastenshteinDistanceFromFullLoopBench();

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public void FastenshteinDistanceFromFullLoopHundred()
    {
        for (int i = 0; i < 100; i++)
        {
            FastenshteinDistanceFromFullLoopBench();
        }
    }

    private static string FastenshteinDistanceFromFullLoopBench()
    {
        var strOut = string.Empty;

        Fastenshtein.Levenshtein lev = new(_sourceString);

        var distance = _levMaxDistance;
        foreach (var item in FilterEntries)
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

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public string RaffinertLevenshteinDistanceFromFullLoop() => RaffinertLevenshteinDistanceFromFullLoopBench();

    [BenchmarkCategory("Levenshtein.DistanceFrom")]
    [Benchmark]
    public void RaffinertLevenshteinDistanceFromFullLoopHundred()
    {
        for (int i = 0; i < 100; i++)
        {
            RaffinertLevenshteinDistanceFromFullLoopBench();
        }
    }

    private static string RaffinertLevenshteinDistanceFromFullLoopBench()
    {
        var strOut = string.Empty;

        using Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);

        var distance = _levMaxDistance;
        foreach (var item in FilterEntries)
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
    #endregion

    #region Levenshtein.Distance
    [GlobalSetup(Target = nameof(FastenshteinDistanceFullLoop))]
    public void SetupFastenshteinDistanceFullLoop()
    {
        var value = FastenshteinDistanceFullLoopBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public string FastenshteinDistanceFullLoop() => FastenshteinDistanceFullLoopBench();

    private static string FastenshteinDistanceFullLoopBench()
    {
        var strOut = string.Empty;

        var distance = _levMaxDistance;
        foreach (var item in FilterEntries)
        {
            int levDistance = Fastenshtein.Levenshtein.Distance(_sourceString, item);
            if (levDistance <= distance)
            {
                strOut = item;
                distance = levDistance - 1;
            }
        }

        return strOut;
    }

    [GlobalSetup(Target = nameof(RaffinertLevenshteinDistanceFullLoop))]
    public void SetupRaffinertLevenshteinDistanceFullLoop()
    {
        var value = RaffinertLevenshteinDistanceFullLoopBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public string RaffinertLevenshteinDistanceFullLoop() => RaffinertLevenshteinDistanceFullLoopBench();

    private static string RaffinertLevenshteinDistanceFullLoopBench()
    {
        var strOut = string.Empty;

        using Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);

        var distance = _levMaxDistance;
        foreach (var item in FilterEntries)
        {
            int levDistance = Raffinert.FuzzySharp.Levenshtein.Distance(_sourceString, item);
            if (levDistance <= distance)
            {
                strOut = item;
                distance = levDistance - 1;
            }
        }

        return strOut;
    }

    [GlobalSetup(Target = nameof(QuickenshteinDistanceFirstResult))]
    public void SetupQuickenshteinDistanceFirstResult()
    {
        var value = QuickenshteinDistanceFirstResultBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public string QuickenshteinDistanceFirstResult() => QuickenshteinDistanceFirstResultBench();

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public string QuickenshteinDistanceFirstResultThreaded() => QuickenshteinDistanceFirstResultBench(multithread: true);

    private static string QuickenshteinDistanceFirstResultBench(bool multithread = false)
    {
        foreach (var item in FilterEntries)
        {
            int levenshteinDistance = multithread ? Quickenshtein.Levenshtein
                .GetDistance(_sourceString, item, Quickenshtein.CalculationOptions.DefaultWithThreading)
                : Quickenshtein.Levenshtein.GetDistance(_sourceString, item);
            if (levenshteinDistance <= _levMaxDistance)
            {
                return item; // Spark fires an additional Projectile
            }
        }

        return string.Empty;
    }

    [GlobalSetup(Target = nameof(QuickenshteinDistanceFullLoop))]
    public void SetupQuickenshteinDistanceFullLoop()
    {
        var value = QuickenshteinDistanceFullLoopBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public string QuickenshteinDistanceFullLoop() => QuickenshteinDistanceFullLoopBench();

    [BenchmarkCategory("Levenshtein.Distance")]
    [Benchmark]
    public void QuickenshteinDistanceFullLoopHundred()
    {
        for (int i = 0; i < 100; i++)
        {
            QuickenshteinDistanceFullLoopBench();
        }
    }

    private static string QuickenshteinDistanceFullLoopBench()
    {
        var strOut = string.Empty;

        var distance = _levMaxDistance;
        foreach (var item in FilterEntries)
        {
            int levDistance = Quickenshtein.Levenshtein.GetDistance(_sourceString, item);
            if (levDistance <= distance)
            {
                strOut = item;
                distance = levDistance - 1;
            }
        }

        return strOut;
    }
    #endregion
    */
    #region XiletradeUseCase
    private static int GetMaxDistance(ReadOnlySpan<char> mod)
    {
        int maxDistance = mod.Length / 8;
        if (maxDistance is 0)
        {
            maxDistance = 1;
        }
        return maxDistance;
    }

    [GlobalSetup(Target = nameof(XiletradeWithRaffinert))]
    public void SetupXiletradeWithRaffiner()
    {
        var value = XiletradeWithRaffinertBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Xiletrade.Levenshtein.DistanceFrom")]
    [Benchmark]
    public string XiletradeWithRaffinert() => XiletradeWithRaffinertBench();

    private static string XiletradeWithRaffinertBench()
    {
        var strOut = string.Empty;

        using Raffinert.FuzzySharp.Levenshtein lev = new(_sourceString);

        var levMaxDistance = GetMaxDistance(_sourceString);
        var closestMatch = FilterEntries
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item) })
                .Where(x => x.Distance <= levMaxDistance)
                .MinBy(x => x.Distance);
        if (closestMatch is not null)
        {
            strOut = closestMatch.Item;
        }

        return strOut;
    }

    [GlobalSetup(Target = nameof(XiletradeWithFastenshtein))]
    public void SetupXiletradeWithFastenshtein()
    {
        var value = XiletradeWithFastenshteinBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Xiletrade.Levenshtein.DistanceFrom")]
    [Benchmark]
    public string XiletradeWithFastenshtein() => XiletradeWithFastenshteinBench();

    private static string XiletradeWithFastenshteinBench()
    {
        var strOut = string.Empty;

        Fastenshtein.Levenshtein lev = new(_sourceString);

        var levMaxDistance = GetMaxDistance(_sourceString);
        var closestMatch = FilterEntries
                .Select(item => new { Item = item, Distance = lev.DistanceFrom(item) })
                .Where(x => x.Distance <= levMaxDistance)
                .MinBy(x => x.Distance);
        if (closestMatch is not null)
        {
            strOut = closestMatch.Item;
        }

        return strOut;
    }

    [GlobalSetup(Target = nameof(XiletradeNoLinqWithFastenshtein))]
    public void SetupXiletradeNoLinqWithFastenshtein()
    {
        var value = XiletradeNoLinqWithFastenshteinBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Xiletrade.Levenshtein.DistanceFrom")]
    [Benchmark]
    public string XiletradeNoLinqWithFastenshtein() => XiletradeNoLinqWithFastenshteinBench();

    private static string XiletradeNoLinqWithFastenshteinBench()
    {
        var bestDistance = int.MaxValue;
        var bestMatch = string.Empty;

        var lev = new Fastenshtein.Levenshtein(_sourceString);
        var levMaxDistance = GetMaxDistance(_sourceString);

        foreach (var item in FilterEntries)
        {
            var distance = lev.DistanceFrom(item);

            if (distance > levMaxDistance)
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMatch = item;

                if (distance is 0)
                    break;
            }
        }

        return bestMatch;
    }

    [GlobalSetup(Target = nameof(XiletradeNoLinqWithRaffinert))]
    public void SetupXiletradeNoLinqWithRaffinert()
    {
        var value = XiletradeNoLinqWithRaffinertBench();
        WriteConsoleAndThrow(value);
    }

    [BenchmarkCategory("Xiletrade.Levenshtein.DistanceFrom")]
    [Benchmark]
    public string XiletradeNoLinqWithRaffinert() => XiletradeNoLinqWithRaffinertBench();

    private static string XiletradeNoLinqWithRaffinertBench()
    {
        var bestDistance = int.MaxValue;
        var bestMatch = string.Empty;

        using var lev = new Raffinert.FuzzySharp.Levenshtein(_sourceString);
        var levMaxDistance = GetMaxDistance(_sourceString);

        foreach (var item in FilterEntries)
        {
            var distance = lev.DistanceFrom(item);

            if (distance > levMaxDistance)
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMatch = item;

                if (distance is 0)
                    break;
            }
        }

        return bestMatch;
    }
    #endregion
}
