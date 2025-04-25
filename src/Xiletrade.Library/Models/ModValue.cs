using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.Models;

internal sealed class ModValue
{
    internal AsyncObservableCollection<AffixFilterEntrie> ListAffix { get; set; } = new();
    public double Min { get; set; } = ModFilter.EMPTYFIELD;
    public double Max { get; set; } = ModFilter.EMPTYFIELD;
}
