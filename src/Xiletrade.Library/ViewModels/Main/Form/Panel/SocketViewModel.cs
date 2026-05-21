using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System;
using System.Text;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class SocketViewModel : ViewModelBase
{
    [ObservableProperty]
    private string redColor;

    [ObservableProperty]
    private string greenColor;

    [ObservableProperty]
    private string blueColor;

    [ObservableProperty]
    private string whiteColor;

    internal SocketViewModel(ItemData item, Dictionary<StatPanel, MinMaxModel> minMax)
    {
        if (!item.IsPoe2)
        {
            string socket = item.Options.Socket;
            int white = socket.Length - socket.Replace("W", string.Empty).Length;
            int red = socket.Length - socket.Replace("R", string.Empty).Length;
            int green = socket.Length - socket.Replace("G", string.Empty).Length;
            int blue = socket.Length - socket.Replace("B", string.Empty).Length;

            var scklinks = socket.Split(' ');
            int lnkcnt = 0;
            for (int s = 0; s < scklinks.Length; s++)
            {
                if (lnkcnt < scklinks[s].Length)
                    lnkcnt = scklinks[s].Length;
            }
            int link = lnkcnt < 3 ? 0 : lnkcnt - (int)Math.Ceiling((double)lnkcnt / 2) + 1;

            redColor = red.ToString();
            greenColor = green.ToString();
            blueColor = blue.ToString();
            whiteColor = white.ToString();

            var search = minMax[StatPanel.CommonSocket];
            search.Selected = link > 4;
            search.Min = (white + red + green + blue).ToString();

            search = minMax[StatPanel.CommonLink];
            search.Selected = link > 4;
            search.Min = link > 0 ? link.ToString() : string.Empty;
            return;
        }

        var runeSocket = item.Options.Socket;
        if (item.Flag.SkillGems)
        {
            var search = minMax[StatPanel.CommonSocketGem];
            int count = runeSocket.Length - runeSocket.Replace("G", string.Empty).Length;
            search.Selected = count >= 4;
            search.Min = count.ToString();
        }
        else
        {
            var search = minMax[StatPanel.CommonSocketRune];
            int count = runeSocket.Split('S').Length - 1;
            var corruptedCond = item.Flag.Corrupted && count >= 1;
            var firstCond = item.Flag.TwoRuneSocketable && count >= 2;
            var secondCond = item.Flag.ThreeRuneSocketable && count >= 3;
            search.Selected = corruptedCond || firstCond || secondCond;
            search.Min = count.ToString();
        }
    }

    internal string GetSocketColors()
    {
        StringBuilder sbColors = new(Resources.Resources.Main210_cbSocketColorsTip);
        sbColors.AppendLine();
        sbColors.Append(Resources.Resources.Main209_cbSocketColors).Append(" : ");
        sbColors.Append(RedColor).Append('R').Append(' ');
        sbColors.Append(GreenColor).Append('G').Append(' ');
        sbColors.Append(BlueColor).Append('B').Append(' ');
        sbColors.Append(WhiteColor).Append('W');

        return sbColors.ToString();
    }
}
