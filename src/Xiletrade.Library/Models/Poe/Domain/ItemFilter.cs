using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class ItemFilter
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
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
        var entry = filterData.GetFilterDataEntry(stat);
        if (entry is not null)
        {
            Id = stat;
            Disabled = false;
            Text = entry.Text;
            Type = entry.Type;
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
