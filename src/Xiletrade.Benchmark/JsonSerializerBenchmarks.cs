using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.IO;
using System.Text;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Benchmark;

/// <summary>
/// This benchmark compares serialization between System.Text.Json (.NET8/9) and Utf8Json 1.3.7 released in 2018.
/// </summary>
/// <remarks>
/// Compare biggest JSON file used by Xiletrade. 
/// </remarks>
[MemoryDiagnoser]
//[NativeMemoryProfiler]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class JsonSerializerBenchmarks
{
    public static byte[] Json { get; private set; }
    public static FilterData Filter { get; private set; }
    public static IJsonSerializer Utf8Serializer { get; private set; } = new Utf8JsonSerializer();
    public static JsonHelper NETSerializer { get; private set; } = new();

    public JsonSerializerBenchmarks()
    {
        LoadFile();
        Filter = Utf8Serializer.Deserialize<FilterData>(Json);
    }

    public static void LoadFile()
    {
        //string path = Path.GetFullPath("Data\\en\\Filters.json");
        string path = Environment.CurrentDirectory.Replace("Benchmark","Library");
        path = path.Substring(0, path.IndexOf("bin")) + "Data\\Lang\\fr-FR\\Filters.json";  // es,fr,br,jp
        if (!File.Exists(path))
        {
            System.Diagnostics.Debug.WriteLine("File not found : " + path);
            return;
        }

        Json = File.ReadAllBytes(path);
        if (Json.AsSpan().StartsWith(Encoding.UTF8.Preamble))
        {
            Json = Json[Encoding.UTF8.Preamble.Length..];
        }
    }

    /// <summary>
    /// deprecated
    /// </summary>
    [Benchmark]
    public void DeserializeWithUtf8()
    {
        Utf8Serializer.Deserialize<FilterData>(Json);
    }

    [Benchmark]
    public void DeserializeWithNET()
    {
        NETSerializer.Deserialize<FilterData>(Json);
    }

    /// <summary>
    /// deprecated
    /// </summary>
    [Benchmark]
    public void SerializeWithUtf8()
    {
        Utf8Serializer.Serialize<FilterData>(Filter);
    }

    [Benchmark]
    public void SerializeWithNET()
    {
        NETSerializer.SerializeToUtf8<FilterData>(Filter);
    }
}
