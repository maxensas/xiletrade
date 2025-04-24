using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class InfluenceViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool shaper;

    [ObservableProperty]
    private bool elder;

    [ObservableProperty]
    private bool crusader;

    [ObservableProperty]
    private bool redeemer;

    [ObservableProperty]
    private bool hunter;

    [ObservableProperty]
    private bool warlord;

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

    internal void SetInfluences(Dictionary<string,string> listOptions)
    {
        ShaperText = Resources.Resources.Main037_Shaper;
        ElderText = Resources.Resources.Main038_Elder;
        CrusaderText = Resources.Resources.Main039_Crusader;
        RedeemerText = Resources.Resources.Main040_Redeemer;
        HunterText = Resources.Resources.Main041_Hunter;
        WarlordText = Resources.Resources.Main042_Warlord;

        Shaper = listOptions[Resources.Resources.General041_Shaper] is Strings.TrueOption;
        Elder = listOptions[Resources.Resources.General042_Elder] is Strings.TrueOption;
        Crusader = listOptions[Resources.Resources.General043_Crusader] is Strings.TrueOption;
        Redeemer = listOptions[Resources.Resources.General044_Redeemer] is Strings.TrueOption;
        Hunter = listOptions[Resources.Resources.General045_Hunter] is Strings.TrueOption;
        Warlord = listOptions[Resources.Resources.General046_Warlord] is Strings.TrueOption;
    }
}
