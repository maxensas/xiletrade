using BenchmarkDotNet.Running;

namespace Xiletrade.Benchmark;

internal class Program
{
    static void Main(string[] args)
    {
        //BenchmarkRunner.Run<JsonSerializerBenchmarks>();
        BenchmarkRunner.Run<ContainModifierBenchmark>();
        //BenchmarkRunner.Run<StringCacheBenchmark>();
        //BenchmarkRunner.Run<ItemDataBenchMark>();
    }
}