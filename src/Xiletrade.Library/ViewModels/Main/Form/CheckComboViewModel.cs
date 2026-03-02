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

    public void Update(InfluenceViewModel influence)
    {
        string influences = influence.GetSate(" & ");
        int checks = influences.AsSpan().Count('&');
        if (influences.Length > 0)
        {
            Text = checks is 0 ? influences : (checks + 1).ToString();
            ToolTip = influences;
            return;
        }
        Text = Resources.Resources.Main036_None;
        ToolTip = null;
    }

    public void Update(ConditionViewModel condition)
    {
        if (!condition.FreePrefix && !condition.FreeSuffix && !condition.SocketColors)
        {
            Text = Resources.Resources.Main036_None;
            ToolTip = null;
            return;
        }

        bool prefixOnly = condition.FreePrefix && !condition.FreeSuffix && !condition.SocketColors;
        bool suffixOnly = condition.FreeSuffix && !condition.FreePrefix && !condition.SocketColors;
        bool colorsOnly = condition.SocketColors && !condition.FreePrefix && !condition.FreeSuffix;
        if (prefixOnly)
        {
            Text = condition.FreePrefixText;
            ToolTip = condition.FreePrefixToolTip;
            return;
        }
        if (suffixOnly)
        {
            Text = condition.FreeSuffixText;
            ToolTip = condition.FreeSuffixToolTip;
            return;
        }
        if (colorsOnly)
        {
            Text = condition.SocketColorsText;
            ToolTip = condition.SocketColorsToolTip;
            return;
        }

        List<KeyValuePair<bool, string>> condList = new();
        condList.Add(new(condition.FreePrefix, condition.FreePrefixToolTip));
        condList.Add(new(condition.FreeSuffix, condition.FreeSuffixToolTip));
        condList.Add(new(condition.SocketColors, condition.SocketColorsToolTip));

        int nbCond = 0;
        StringBuilder toolTip = new();
        foreach (var cond in condList)
        {
            if (cond.Key)
            {
                nbCond++;
                if (toolTip.Length > 0)
                {
                    toolTip.AppendLine(); // "\n"
                }
                toolTip.Append(cond.Value);
            }
        }

        if (nbCond > 0)
        {
            Text = nbCond.ToString();
            ToolTip = toolTip.ToString();
        }
    }
}
