using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.IO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;

namespace Xiletrade.Benchmark;

/// <summary>
/// Exact text lookup in the filters (FilterDataExtensions.ContainModifier) : scan of all the entries vs hash index.
/// </summary>
/// <remarks>
/// Found : last entry of the last filter (worst case of the scan for a text found).
/// Missing : raw modifier with its values, never an entry text (full scan, the most frequent case in ModInfo).
/// Build : creation of the index, its 'Allocated' column is the memory kept by the index.
/// </remarks>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[RankColumn]
public class ContainModifierBenchmark
{
    private const string MISSING = "Adds 2 to 23 Lightning Damage to Spells";

    private FilterData _filterPoe1;
    private FilterData _filterPoe2;
    private string _foundPoe1;
    private string _foundPoe2;
    private HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _lookupPoe1;
    private HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _lookupPoe2;

    [GlobalSetup]
    public void Setup()
    {
        var json = new JsonHelper();
        _filterPoe1 = LoadFilter(json, "Filters.json", arrangeVersion: 0);
        _filterPoe2 = LoadFilter(json, "FiltersTwo.json", arrangeVersion: 1);
        _foundPoe1 = _filterPoe1.Result[^1].Entries[^1].Text;
        _foundPoe2 = _filterPoe2.Result[^1].Entries[^1].Text;
        _lookupPoe1 = BuildIndex(_filterPoe1).GetAlternateLookup<ReadOnlySpan<char>>();
        _lookupPoe2 = BuildIndex(_filterPoe2).GetAlternateLookup<ReadOnlySpan<char>>();
    }

    [BenchmarkCategory("Poe1.Missing")]
    [Benchmark]
    public bool Scan_Missing_Poe1() => Scan(_filterPoe1, MISSING);

    [BenchmarkCategory("Poe1.Missing")]
    [Benchmark]
    public bool Index_Missing_Poe1() => _lookupPoe1.Contains(MISSING);

    [BenchmarkCategory("Poe1.Found")]
    [Benchmark]
    public bool Scan_Found_Poe1() => Scan(_filterPoe1, _foundPoe1);

    [BenchmarkCategory("Poe1.Found")]
    [Benchmark]
    public bool Index_Found_Poe1() => _lookupPoe1.Contains(_foundPoe1);

    [BenchmarkCategory("Poe1.Build")]
    [Benchmark]
    public HashSet<string> Index_Build_Poe1() => BuildIndex(_filterPoe1);

    [BenchmarkCategory("Poe2.Missing")]
    [Benchmark]
    public bool Scan_Missing_Poe2() => Scan(_filterPoe2, MISSING);

    [BenchmarkCategory("Poe2.Missing")]
    [Benchmark]
    public bool Index_Missing_Poe2() => _lookupPoe2.Contains(MISSING);

    [BenchmarkCategory("Poe2.Found")]
    [Benchmark]
    public bool Scan_Found_Poe2() => Scan(_filterPoe2, _foundPoe2);

    [BenchmarkCategory("Poe2.Found")]
    [Benchmark]
    public bool Index_Found_Poe2() => _lookupPoe2.Contains(_foundPoe2);

    [BenchmarkCategory("Poe2.Build")]
    [Benchmark]
    public HashSet<string> Index_Build_Poe2() => BuildIndex(_filterPoe2);

    // current implementation of FilterDataExtensions.ContainModifier (sequenceEquality: true)
    private static bool Scan(FilterData filter, ReadOnlySpan<char> modText)
    {
        var results = filter.Result;
        for (int i = 0; i < results.Length; i++)
        {
            var entries = results[i].Entries;
            for (int j = 0; j < entries.Length; j++)
            {
                if (entries[j].Text.AsSpan().SequenceEqual(modText))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // same as FilterDataExtensions.EntryTextIndex
    private static HashSet<string> BuildIndex(FilterData filter)
    {
        var texts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var result in filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                texts.Add(entry.Text ?? string.Empty);
            }
        }
        return texts;
    }

    // english filters copied with the library data, deserialized and arranged as DataManagerService does
    // (no service : DataManagerService needs a logger, a message service and a view manager)
    private static FilterData LoadFilter(JsonHelper json, string fileName, int arrangeVersion)
    {
        var path = Path.Combine(DataManagerService.DataDirectory, "Lang", "en-US", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Filter file not found next to the benchmark", path);
        }
        using var stream = File.OpenRead(path);
        return json.Deserialize<FilterData>(stream).ArrangeFilter(arrangeVersion);
    }
}
