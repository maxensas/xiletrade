using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.IO;
using System.Linq;
using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.SimilarityRatio;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite;
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
    public static JsonHelper NETSerializer { get; } = new();

    public FuzzBenchmark()
    {
        LoadFile();
        Filter = NETSerializer.Deserialize<FilterData>(Json);
    }

    public static void LoadFile()
    {
        //string path = Path.GetFullPath("Data\\en\\Filters.json");
        string path = Environment.CurrentDirectory.Replace("Benchmark", "Library");
        path = path.Substring(0, path.IndexOf("bin")) + "Data\\Lang\\fr-FR\\Filters.json";  // es,fr,br,jp
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

    [Benchmark]
    public void FuzzProcessExtractOne()
    {
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            var res = Process.ExtractOne(str, entrySeek,  static s => s, ScorerCache.Get<WeightedRatioScorer>(), cutoff: 94);
            if (res is not null)
            {
                strOut = res.Value; // equal "Spark fires an additional Projectile"
            }
        }
    }

    [Benchmark]
    public void FuzzProcessExtractTop()
    {
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            var res = Process.ExtractTop(str, entrySeek, static s => s, ScorerCache.Get<WeightedRatioScorer>(), limit: 1, cutoff: 94);
            if (res is not null)
            {
                strOut = res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
            }
        }
    }

    [Benchmark]
    public void FuzzProcessExtractAll()
    {
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            var res = Process.ExtractAll(str, entrySeek, static s => s, ScorerCache.Get<WeightedRatioScorer>(), cutoff: 94);
            if (res is not null)
            {
                strOut = res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
            }
        }
    }

    [Benchmark]
    public void FuzzProcessExtractSorted()
    {
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            var res = Process.ExtractSorted(str, entrySeek, static s => s, ScorerCache.Get<WeightedRatioScorer>(), cutoff: 94);
            if (res is not null)
            {
                strOut = res.FirstOrDefault()?.Value; // equal "Spark fires an additional Projectile"
            }
        }
    }

    [Benchmark]
    public void FuzzWeightedRatioFirstResult()
    {
        string str = "Spark fires 3 additional Projectiles";
        string strOut = string.Empty;
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            int cutoff = 94;
            foreach (var entry in entrySeek)
            {
                if (Fuzz.WeightedRatio(str, entry) >= cutoff)
                {
                    strOut = entry; // equal "Spark fires an additional Projectile"
                    break;
                }
            }
        }
    }

    [Benchmark]
    public void FastenshteinFirstResult()
    {
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
    }

    [Benchmark]
    public void FuzzLevenshteinFirstResult()
    {
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
            Levenshtein lev = new(str);
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
    }

    [Benchmark]
    public void FastenshteinFullLoop()
    {
        var strOut = string.Empty;
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

            var distance = maxDistance;
            foreach (var item in entrySeek)
            {
                int levDistance = lev.DistanceFrom(item);
                if (levDistance <= distance)
                {
                    strOut = item;
                    distance = levDistance - 1;
                }
            }
        }
    }

    [Benchmark]
    public void FuzzLevenshteinFullLoop()
    {
        var strOut = string.Empty;
        int maxDistance = 5;
        string str = "Spark fires 3 additional Projectiles";
        var entrySeek =
            from result in Filter.Result
            from filter in result.Entries
            select filter.Text;
        var seek = entrySeek.FirstOrDefault(x => x.Contains(str, StringComparison.Ordinal));
        if (seek is null)
        {
            Levenshtein lev = new(str);

            var distance = maxDistance;
            foreach (var item in entrySeek)
            {
                int levDistance = lev.DistanceFrom(item);
                if (levDistance <= distance)
                {
                    strOut = item;
                    distance = levDistance - 1;
                }
            }
        }
    }
}
