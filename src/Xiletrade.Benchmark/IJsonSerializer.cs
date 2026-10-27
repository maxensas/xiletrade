namespace Xiletrade.Benchmark;

public interface IJsonSerializer
{
    public byte[] Serialize<T>(object obj) where T : class;
    public T Deserialize<T>(byte[] utf8Json) where T : class;
}