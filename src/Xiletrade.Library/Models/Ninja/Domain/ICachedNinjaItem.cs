using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

public interface ICachedNinjaItem<TContract> where TContract : class, new()
{
    DateTime Creation { get; set; }
    bool CheckValidity();
    void DeserializeAndSetJson(string json);
    TContract GetJson();
}