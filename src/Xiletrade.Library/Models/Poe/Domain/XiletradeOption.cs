using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class XiletradeOption
{
    private double _min = ModFilter.EMPTYFIELD;
    private double _max = ModFilter.EMPTYFIELD;

    internal bool Enable { get; set; }
    internal double? Min { get { return _min.IsNotEmpty() ? _min : null; } set { _min = value ?? ModFilter.EMPTYFIELD; } }
    internal double? Max { get { return _max.IsNotEmpty() ? _max : null; } set { _max = value ?? ModFilter.EMPTYFIELD; } }
}
