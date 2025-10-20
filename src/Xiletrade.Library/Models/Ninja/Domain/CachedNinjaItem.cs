using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal abstract class CachedNinjaItem<TContract>(string name) : ICachedNinjaItem<TContract> where TContract : class, new()
{
    private readonly int _cacheTime = 30;
    public DateTime Creation { get; set; } = DateTime.MinValue;
    public string Name { get; } = name;
    internal TContract Json { get; set; }

    public void SetJson(TContract json)
    {
        Json = json;
        Creation = DateTime.UtcNow;
    }

    public bool IsCacheValid()
        => Creation.AddMinutes(_cacheTime) > DateTime.UtcNow;

    public virtual TContract GetJson() => Json;
}
