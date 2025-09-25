using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

public interface ICachedNinjaItem<TContract> where TContract : class, new()
{
    DateTime Creation { get; set; }
    string Name { get; }
    bool IsCacheValid();
    void DeserializeAndSetJson(string json);
    TContract GetJson();
}