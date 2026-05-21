using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Models.Poe.Domain.Parser;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class InfluenceViewModel(ItemFlag flag) : ViewModelBase
{
    [ObservableProperty]
    private bool shaper = flag.InfluenceShaper;

    [ObservableProperty]
    private bool elder = flag.InfluenceElder;

    [ObservableProperty]
    private bool crusader = flag.InfluenceCrusader;

    [ObservableProperty]
    private bool redeemer = flag.InfluenceRedeemer;

    [ObservableProperty]
    private bool hunter = flag.InfluenceHunter;

    [ObservableProperty]
    private bool warlord = flag.InfluenceWarlord;

    [ObservableProperty]
    private string shaperText = Resources.Resources.Main037_Shaper;

    [ObservableProperty]
    private string elderText = Resources.Resources.Main038_Elder;

    [ObservableProperty]
    private string crusaderText = Resources.Resources.Main039_Crusader;

    [ObservableProperty]
    private string redeemerText = Resources.Resources.Main040_Redeemer;

    [ObservableProperty]
    private string hunterText = Resources.Resources.Main041_Hunter;

    [ObservableProperty]
    private string warlordText = Resources.Resources.Main042_Warlord;

    internal string GetSate(string delimiter)
    {
        string influences = string.Empty;
        foreach (var inf in GetDictionary())
        {
            if (inf.Value)
            {
                if (influences.Length > 0) influences += delimiter;
                influences += inf.Key;
            }
        }
        return influences;
    }

    private Dictionary<string, bool> GetDictionary()
    {
        return new Dictionary<string, bool>()
        {
            { ShaperText, Shaper },
            { ElderText, Elder },
            { CrusaderText, Crusader },
            { RedeemerText, Redeemer },
            { WarlordText, Warlord },
            { HunterText, Hunter }
        };
    }
}
