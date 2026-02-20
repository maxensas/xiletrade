namespace Xiletrade.Library.Models.Poe.Domain;

public sealed record CurrencyInfo
{
    public string Name { get; }
    public string Uri { get; } 
    public bool IsPoe2 { get; }

    public CurrencyInfo(string name, bool isPoe2)
    {
        Name = name;
        IsPoe2 = isPoe2;
    }

    public CurrencyInfo(string name, string uri, bool isPoe2)
    {
        Name = name;
        Uri = uri;
        IsPoe2 = isPoe2;
    }
}
