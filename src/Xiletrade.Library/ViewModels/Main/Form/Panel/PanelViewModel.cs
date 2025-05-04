using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Parser;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blighRavaged;

    [ObservableProperty]
    private string synthesisBlightLabel = "Synthblight";// = string.Empty;

    [ObservableProperty]
    private string blighRavagedtLabel = "Ravaged";// = string.Empty;

    [ObservableProperty]
    private string facetorMin = string.Empty;

    [ObservableProperty]
    private string facetorMax = string.Empty;

    [ObservableProperty]
    private bool useBorderThickness = true;

    [ObservableProperty]
    private CommonStatViewModel common = new();

    [ObservableProperty]
    private DefenseViewModel defense = new();

    [ObservableProperty]
    private DamageViewModel damage = new();

    [ObservableProperty]
    private TotalStatViewModel total = new();

    [ObservableProperty]
    private RewardViewModel reward = new();

    [ObservableProperty]
    private SanctumViewModel sanctum = new();

    [ObservableProperty]
    private MapViewModel map = new();

    [ObservableProperty]
    private PanelRowViewModel row = new();

    [ObservableProperty]
    private PanelColViewModel col = new();

    internal void Update(ItemData item)
    {
        string specifier = "G";
        if (item.Stats.Resistance > 0)
        {
            Total.Resistance.Min = item.Stats.Resistance.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (item.Stats.Life > 0)
        {
            Total.Life.Min = item.Stats.Life.ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (item.Stats.EnergyShield > 0)
        {
            Total.GlobalEs.Min = item.Stats.EnergyShield.ToString(specifier, CultureInfo.InvariantCulture);
        }

        if (item.Flag.SanctumResearch)
        {
            var resolve = item.Option[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
            if (resolve.Length is 2)
            {
                Sanctum.Resolve.Min = resolve[0];
                Sanctum.MaximumResolve.Max = resolve[1];
            }
            Sanctum.Inspiration.Min = item.Option[Resources.Resources.General115_SanctumInspiration];
            Sanctum.Aureus.Min = item.Option[Resources.Resources.General116_SanctumAureus];
        }
    }
}
