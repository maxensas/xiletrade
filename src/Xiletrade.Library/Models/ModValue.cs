using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class ModValue
{
    internal AsyncObservableCollection<AffixFilterEntrie> ListAffix { get; set; } = new();
    public double Min { get; set; } = Modifier.EMPTYFIELD;
    public double Max { get; set; } = Modifier.EMPTYFIELD;
}
