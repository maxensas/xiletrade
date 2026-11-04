using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Serialization.SourceGeneration;
using Xiletrade.Library.Services;
using StringsFile = Xiletrade.Library.Shared.Strings.File;

namespace Xiletrade.Benchmark;

/// <summary>
/// Loads the data files as DataManagerService does (same files, same contexts), with and without StringCache,
/// and compares the memory kept once loaded, the memory allocated by the loading and its duration.
/// </summary>
/// <remarks>
/// Not a BenchmarkDotNet benchmark : its 'Allocated' column counts what is allocated, not what is kept.
/// The files are read before the measures (only the deserialization is measured) and loaded one after the other
/// (DataManagerService loads them in parallel : same memory, shorter duration).
/// Run in Release : dotnet run -c Release --project Xiletrade.Benchmark -- stringcache
/// </remarks>
internal static class StringCacheComparison
{
    private const int RUNS = 7;

    private sealed record DataFile(string Name, byte[] Json, Func<JsonHelper, byte[], SourceGenerationContext, object> Load);

    private sealed record Measure(long Retained, long Allocated, double Milliseconds, (int References, int Values, int Instances) FilterStrings);

    internal static void Run()
    {
        var json = new JsonHelper();
        foreach (var (lang, isPoe2) in new[] { ("en-US", false), ("fr-FR", false), ("en-US", true), ("fr-FR", true) })
        {
            var files = GetFiles(lang, isPoe2);

            // warm-up : JIT and metadata of the contexts
            LoadOnce(json, files, useCache: false);
            LoadOnce(json, files, useCache: true);

            var without = new List<Measure>();
            var with = new List<Measure>();
            for (int i = 0; i < RUNS; i++) // alternated : same conditions for both
            {
                without.Add(LoadOnce(json, files, useCache: false));
                with.Add(LoadOnce(json, files, useCache: true));
            }

            Print(lang, isPoe2, files, without, with);
        }
    }

    private static Measure LoadOnce(JsonHelper json, List<DataFile> files, bool useCache)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetTotalMemory(forceFullCollection: true);
        long allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);

        var watch = Stopwatch.StartNew();
        // as DataManagerService : one cache shared by all the files loaded together, released with the loading
        var context = useCache ? json.CreateInterningContext(new StringCache()) : null;
        var data = new object[files.Count];
        for (int i = 0; i < files.Count; i++)
        {
            data[i] = files[i].Load(json, files[i].Json, context);
        }
        watch.Stop();
        context = null;

        long allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
        long retained = GC.GetTotalMemory(forceFullCollection: true) - before;

        // counted after the measures, the loaded data is then released (not kept between the loads)
        int filterIndex = files.FindIndex(f => f.Name.StartsWith("Filters", StringComparison.Ordinal));
        var filterStrings = CountFilterStrings((FilterData)data[filterIndex]);

        return new(retained, allocated, watch.Elapsed.TotalMilliseconds, filterStrings);
    }

    // same files as DataManagerService.Initialize (gateway files excluded)
    private static List<DataFile> GetFiles(string lang, bool isPoe2)
    {
        var basePath = DataManagerService.DataDirectory;
        var langPath = Path.Combine(basePath, "Lang", lang);
        var enPath = Path.Combine(basePath, "Lang", "en-US");
        int arrange = isPoe2 ? 1 : 0;

        var files = new List<DataFile>
        {
            Create<DivTiersData>(Path.Combine(basePath, StringsFile.Divination)),
            Create<DustData>(Path.Combine(basePath, StringsFile.DustLevel)),
            Create<SearchPresetData>(Path.Combine(basePath, StringsFile.SearchPreset)),
            Create<BaseData>(Path.Combine(langPath, isPoe2 ? StringsFile._bases2 : StringsFile._bases1)),
            Create<ModData>(Path.Combine(langPath, isPoe2 ? StringsFile._mods2 : StringsFile._mods1)),
            Create<BaseData>(Path.Combine(langPath, StringsFile.Monsters)),
            Create<WordData>(Path.Combine(langPath, isPoe2 ? StringsFile._words2 : StringsFile._words1)),
            Create<GemData>(Path.Combine(langPath, StringsFile.Gems)),
            Create<ParserData>(Path.Combine(langPath, StringsFile.ParsingRules)),
            Create<FilterData>(Path.Combine(langPath, isPoe2 ? StringsFile._filters2 : StringsFile._filters1), f => f.ArrangeFilter(arrange)),
            Create<CurrencyResult>(Path.Combine(langPath, isPoe2 ? StringsFile._currency2 : StringsFile._currency1)),
            Create<ItemTypeResult>(Path.Combine(langPath, isPoe2 ? StringsFile._items2 : StringsFile._items1)),
        };

        if (lang is not "en-US") // english files also loaded when another language is used
        {
            files.Add(Create<FilterData>(Path.Combine(enPath, isPoe2 ? StringsFile._filters2 : StringsFile._filters1), f => f.ArrangeFilter(arrange)));
            files.Add(Create<CurrencyResult>(Path.Combine(enPath, isPoe2 ? StringsFile._currency2 : StringsFile._currency1)));
            files.Add(Create<ItemTypeResult>(Path.Combine(enPath, isPoe2 ? StringsFile._items2 : StringsFile._items1)));
        }
        return files;
    }

    private static DataFile Create<T>(string path, Func<T, T> arrange = null) where T : class
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Data file not found next to the benchmark", path);
        }
        return new(Path.GetFileName(path), File.ReadAllBytes(path), (json, bytes, context) =>
        {
            var data = json.Deserialize<T>(bytes, context: context);
            return arrange is null ? data : arrange(data);
        });
    }

    private static void Print(string lang, bool isPoe2, List<DataFile> files, List<Measure> without, List<Measure> with)
    {
        long jsonSize = files.Sum(f => (long)f.Json.Length);
        Console.WriteLine();
        Console.WriteLine($"=== {lang} / {(isPoe2 ? "PoE2" : "PoE1")} : {files.Count} files, {Mb(jsonSize)} of json, median of {RUNS} loads ===");
        Console.WriteLine($"{"",-28}{"Without cache",16}{"With cache",16}{"Difference",22}");
        Row("Kept after loading", Median(without, m => m.Retained), Median(with, m => m.Retained));
        Row("Allocated by loading", Median(without, m => m.Allocated), Median(with, m => m.Allocated));

        double msWithout = Median(without, m => m.Milliseconds);
        double msWith = Median(with, m => m.Milliseconds);
        Console.WriteLine($"{"Loading duration",-28}{msWithout,13:0.0} ms{msWith,13:0.0} ms{msWith - msWithout,15:+0.0;-0.0} ms ({Percent(msWith, msWithout)})");

        // string instances of the filters (language file) : the duplicates are shared by the cache
        var (references, values, instancesWithout) = without[0].FilterStrings;
        var (_, _, instancesWith) = with[0].FilterStrings;
        Console.WriteLine($"Filter strings (id, text, type, part, options) : {references} references, {values} distinct values,");
        Console.WriteLine($"  string instances : {instancesWithout} without cache, {instancesWith} with cache");
    }

    private static void Row(string label, double without, double with)
        => Console.WriteLine($"{label,-28}{Mb((long)without),16}{Mb((long)with),16}{Mb((long)(with - without), signed: true),15} ({Percent(with, without)})");

    private static (int References, int Values, int Instances) CountFilterStrings(FilterData filter)
    {
        int references = 0;
        var values = new HashSet<string>(StringComparer.Ordinal);
        var instances = new HashSet<string>(ReferenceEqualityComparer.Instance);

        void Add(string s)
        {
            if (s is null)
            {
                return;
            }
            references++;
            values.Add(s);
            instances.Add(s);
        }

        foreach (var result in filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                Add(entry.ID);
                Add(entry.Text);
                Add(entry.Type);
                Add(entry.Part);
                foreach (var option in entry.Option?.Options ?? [])
                {
                    Add(option.Text);
                }
            }
        }
        return (references, values.Count, instances.Count);
    }

    private static double Median(List<Measure> measures, Func<Measure, double> value)
    {
        var sorted = measures.Select(value).Order().ToArray();
        return sorted[sorted.Length / 2];
    }

    private static string Mb(long bytes, bool signed = false)
        => (bytes / 1024d / 1024d).ToString(signed ? "+0.00;-0.00" : "0.00") + " MB";

    private static string Percent(double with, double without)
        => without is 0 ? "n/a" : ((with - without) / without).ToString("+0.0%;-0.0%");
}
