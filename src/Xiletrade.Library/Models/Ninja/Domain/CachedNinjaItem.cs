using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal abstract class CachedNinjaItem<TContract>(string name) : ICachedNinjaItem<TContract> where TContract : class, new()
{
    private readonly int _cacheTime = 30;
    public DateTime Creation { get; set; } = DateTime.MinValue;
    public string Name { get; } = name;
    internal TContract Json { get; set; }

    public void DeserializeAndSetJson(string json)
    {
        try
        {
            Json = Shared.Json.Deserialize<TContract>(json);
            Creation = DateTime.UtcNow;
        }
        catch (Exception)
        { 
            throw;
        }
    }

    public bool IsCacheValid()
        => Creation.AddMinutes(_cacheTime) > DateTime.UtcNow;

    public virtual TContract GetJson() => Json;
}
