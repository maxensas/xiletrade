using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal abstract class CachedNinjaItem<TContract> : ICachedNinjaItem where TContract : class, new()
{
    private readonly int _cacheTime = 30;
    public DateTime Creation { get; set; } = DateTime.MinValue;
    internal TContract Json { get; set; } 

    public void DeserializeAndSetJson(string json)
    {
        Json = Shared.Json.Deserialize<TContract>(json);
        Creation = DateTime.UtcNow;
    }

    public bool CheckValidity()
        => Creation == DateTime.MinValue || Creation.AddMinutes(_cacheTime) < DateTime.UtcNow;

    public abstract object GetJson();
}
