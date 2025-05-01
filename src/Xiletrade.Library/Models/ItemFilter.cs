using System.Linq;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

public sealed class ItemFilter
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double Max { get; set; }
    public double Min { get; set; }
    public bool Disabled { get; set; }
    public int Option { get; set; }
    public bool IsNull { get; set; }

    public ItemFilter()
    {

    }

    public ItemFilter(string id)
    {
        Id = id;
    }

    public ItemFilter(string stat, double min, double max)
    {
        var entry = from result in DataManager.Filter.Result
                      from filter in result.Entries
                      where filter.ID == stat
                      select filter;
        if (entry.Any())
        {
            Id = stat;
            Disabled = false;
            Text = entry.First().Text;
            Min = min;
            Max = max;
        }
    }

    public ItemFilter(string stat, string strMin, string strMax) 
        : this(stat, strMin.ToDoubleEmptyField(), strMax.ToDoubleEmptyField())
    {

    }
}
