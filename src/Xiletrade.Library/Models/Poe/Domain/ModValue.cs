using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class ModValue
{
    internal AsyncObservableCollection<AffixFilterEntrie> ListAffix { get; set; } = new();
    public double Min { get; set; } = ModFilter.EMPTYFIELD;
    public double Max { get; set; } = ModFilter.EMPTYFIELD;
}
