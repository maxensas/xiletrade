using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
/// Loading of the data files as DataManagerService does (same files, same contexts), with and without StringCache.
/// </summary>
/// <remarks>
/// Mean / Allocated : duration and memory allocated by a loading (files read before : deserialization only, 
/// loaded one after the other while DataManagerService loads them in parallel).
/// Kept : memory kept once loaded, what the cache is made for (BenchmarkDotNet only measures what is allocated).
/// Instances : string instances of the filters (id, text, type, part, options), shared by the cache.
/// </remarks>
[MemoryDiagnoser]
[Config(typeof(Config))]
public class StringCacheBenchmark
{
    [Params("en-US", "fr-FR")]
    public string Lang { get; set; }

    [Params(false, true)]
    public bool Poe2 { get; set; }

    [Params(false, true)]
    public bool UseCache { get; set; }

    private JsonHelper _json;
    private List<DataFile> _files;

    [GlobalSetup]
    public void Setup()
    {
        _json = new();
        _files = DataFile.GetFiles(Lang, Poe2);
    }

    [Benchmark]
    public object[] Load() => DataFile.LoadAll(_json, _files, UseCache);

    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddColumn(new KeptColumn(), new InstancesColumn());
        }
    }
}

/// <summary>A data file read in memory and its deserialization, as done by DataManagerService.</summary>
internal sealed record DataFile(string Name, byte[] Json, Func<JsonHelper, byte[], SourceGenerationContext, object> Load)
{
    private const int KEPT_MEASURES = 5;
    private static readonly ConcurrentDictionary<(string, bool, bool), (long Kept, int Instances)> _kept = new();

    /// <summary>One cache shared by all the files loaded together, released with the loading (as DataManagerService).</summary>
    internal static object[] LoadAll(JsonHelper json, List<DataFile> files, bool useCache)
    {
        var context = useCache ? json.CreateInterningContext(new StringCache()) : null;
        var data = new object[files.Count];
        for (int i = 0; i < files.Count; i++)
        {
            data[i] = files[i].Load(json, files[i].Json, context);
        }
        return data;
    }

    // same files as DataManagerService.Initialize (gateway files excluded)
    internal static List<DataFile> GetFiles(string lang, bool isPoe2)
    {
        var basePath = DataManagerService.DataDirectory;
        var langPath = Path.Combine(basePath, "Lang", lang);
        var enPath = Path.Combine(basePath, "Lang", "en-US");
        int arrange = isPoe2 ? 1 : 0;

        var files = new List<DataFile>
        {
            Create<DivTiersData>(Path.Combine(basePath, StringsFile.Divination)),
            Create<DustData>(Path.Combine(basePath, StringsFile.DustLevel)),
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

    /// <summary>
    /// Memory kept once loaded and string instances of the filters, measured in the process of BenchmarkDotNet
    /// (median of full collections around a loading, computed once by case).
    /// </summary>
    internal static (long Kept, int Instances) GetKept(BenchmarkCase benchmarkCase)
    {
        var lang = (string)benchmarkCase.Parameters["Lang"];
        var isPoe2 = (bool)benchmarkCase.Parameters["Poe2"];
        var useCache = (bool)benchmarkCase.Parameters["UseCache"];
        return _kept.GetOrAdd((lang, isPoe2, useCache), _ => MeasureKept(lang, isPoe2, useCache));
    }

    private static (long Kept, int Instances) MeasureKept(string lang, bool isPoe2, bool useCache)
    {
        var json = new JsonHelper();
        var files = GetFiles(lang, isPoe2);
        LoadAll(json, files, useCache); // warm-up : metadata of the contexts

        var kept = new long[KEPT_MEASURES];
        int instances = 0;
        for (int i = 0; i < KEPT_MEASURES; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long before = GC.GetTotalMemory(forceFullCollection: true);
            var data = LoadAll(json, files, useCache);
            kept[i] = GC.GetTotalMemory(forceFullCollection: true) - before;

            int filterIndex = files.FindIndex(f => f.Name.StartsWith("Filters", StringComparison.Ordinal));
            instances = CountStringInstances((FilterData)data[filterIndex]);
        }
        Array.Sort(kept);
        return (kept[KEPT_MEASURES / 2], instances);
    }

    private static int CountStringInstances(FilterData filter)
    {
        var instances = new HashSet<string>(ReferenceEqualityComparer.Instance);
        foreach (var result in filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                Add(instances, entry.ID);
                Add(instances, entry.Text);
                Add(instances, entry.Type);
                Add(instances, entry.Part);
                foreach (var option in entry.Option?.Options ?? [])
                {
                    Add(instances, option.Text);
                }
            }
        }
        return instances.Count;
    }

    private static void Add(HashSet<string> instances, string value)
    {
        if (value is not null)
        {
            instances.Add(value);
        }
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
}

/// <summary>Memory kept once the files are loaded.</summary>
internal sealed class KeptColumn : IColumn
{
    public string Id => nameof(KeptColumn);
    public string ColumnName => "Kept";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "Memory kept once the files are loaded (median of 5 full collections, process of BenchmarkDotNet)";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        => (DataFile.GetKept(benchmarkCase).Kept / 1024d / 1024d).ToString("0.00") + " MB";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;
}

/// <summary>String instances of the filters : equal to their distinct values when they are all shared.</summary>
internal sealed class InstancesColumn : IColumn
{
    public string Id => nameof(InstancesColumn);
    public string ColumnName => "Filter strings";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 1;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "String instances of the filters of the language (id, text, type, part, options)";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        => DataFile.GetKept(benchmarkCase).Instances.ToString();

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;
}
