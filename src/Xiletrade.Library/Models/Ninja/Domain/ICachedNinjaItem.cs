using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal interface ICachedNinjaItem
{
    DateTime Creation { get; set; }
    bool CheckValidity();
    void DeserializeAndSetJson(string json);
    object GetJson();
}