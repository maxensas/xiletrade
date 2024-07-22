namespace Xiletrade.Benchmark;

public interface IJsonSerializer
{
    public string Serialize<T>(object obj) where T : class;
    public T Deserialize<T>(string strData) where T : class;
}