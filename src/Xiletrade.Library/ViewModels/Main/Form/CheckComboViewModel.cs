using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class CheckComboViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = Resources.Resources.Main036_None;

    [ObservableProperty]
    private string toolTip = null;

    [ObservableProperty]
    private string none = Resources.Resources.Main036_None;

    public CheckComboViewModel(InfluenceViewModel influence)
    {
        string influences = influence.GetSate(" & ");
        int checks = influences.AsSpan().Count('&');
        if (influences.Length > 0)
        {
            text = checks is 0 ? influences : (checks + 1).ToString();
            toolTip = influences;
            return;
        }
        text = Resources.Resources.Main036_None;
        toolTip = null;
    }

    public CheckComboViewModel(ConditionViewModel condition)
    {
        if (!condition.FreePrefix && !condition.FreeSuffix && !condition.SocketColors)
        {
            text = Resources.Resources.Main036_None;
            toolTip = null;
            return;
        }

        bool prefixOnly = condition.FreePrefix && !condition.FreeSuffix && !condition.SocketColors;
        bool suffixOnly = condition.FreeSuffix && !condition.FreePrefix && !condition.SocketColors;
        bool colorsOnly = condition.SocketColors && !condition.FreePrefix && !condition.FreeSuffix;
        if (prefixOnly)
        {
            text = condition.FreePrefixText;
            toolTip = condition.FreePrefixToolTip;
            return;
        }
        if (suffixOnly)
        {
            text = condition.FreeSuffixText;
            toolTip = condition.FreeSuffixToolTip;
            return;
        }
        if (colorsOnly)
        {
            text = condition.SocketColorsText;
            toolTip = condition.SocketColorsToolTip;
            return;
        }

        List<KeyValuePair<bool, string>> condList = new();
        condList.Add(new(condition.FreePrefix, condition.FreePrefixToolTip));
        condList.Add(new(condition.FreeSuffix, condition.FreeSuffixToolTip));
        condList.Add(new(condition.SocketColors, condition.SocketColorsToolTip));

        int nbCond = 0;
        StringBuilder sbTip = new();
        foreach (var cond in condList)
        {
            if (cond.Key)
            {
                nbCond++;
                if (sbTip.Length > 0)
                {
                    sbTip.AppendLine(); // "\n"
                }
                sbTip.Append(cond.Value);
            }
        }

        if (nbCond > 0)
        {
            text = nbCond.ToString();
            toolTip = sbTip.ToString();
        }
    }
}
