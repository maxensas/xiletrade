using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

public interface ICachedNinjaItem
{
    DateTime Creation { get; set; }
    string Name { get; }
    bool IsCacheValid();
}