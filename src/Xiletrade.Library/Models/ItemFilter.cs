using System.Linq;
using Xiletrade.Library.Models.Serializable;
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

    public ItemFilter(FilterData filterData, string stat, double min, double max)
    {
        var entry = from result in filterData.Result
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

    public ItemFilter(FilterData filterData, string stat, string strMin, string strMax) 
        : this(filterData, stat, strMin.ToDoubleEmptyField(), strMax.ToDoubleEmptyField())
    {

    }

    public ItemFilter(FilterData filterData, string stat, double min, string strMax)
        : this(filterData, stat, min, strMax.ToDoubleEmptyField())
    {

    }
}
