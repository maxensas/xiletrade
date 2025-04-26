using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using System;
using Xiletrade.Library.Models.Parser;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class CommonStatViewModel : ViewModelBase
{
    [ObservableProperty]
    private string itemLevelLabel = string.Empty;

    [ObservableProperty]
    private MinMaxViewModel itemLevel = new();

    [ObservableProperty]
    private MinMaxViewModel quality = new();

    [ObservableProperty]
    private SocketViewModel sockets = new();

    [ObservableProperty]
    private MinMaxViewModel runeSockets = new();

    internal void Update(ItemData item, bool isPoe2)
    {
        if (!isPoe2)
        {
            string socket = item.Option[Resources.Resources.General036_Socket];
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

            Sockets.RedColor = red.ToString();
            Sockets.GreenColor = green.ToString();
            Sockets.BlueColor = blue.ToString();
            Sockets.WhiteColor = white.ToString();
            Sockets.SocketMin = (white + red + green + blue).ToString();
            Sockets.LinkMin = link > 0 ? link.ToString() : string.Empty;
            Sockets.Selected = link > 4;
        }

        if (isPoe2)
        {
            string socket = item.Option[Resources.Resources.General036_Socket];
            int count = socket.Split('S').Length - 1;
            RuneSockets.Selected = item.Flag.Corrupted && count >= 1;
            RuneSockets.Min = count.ToString();
            if (item.Flag.Corrupted)
            {
                RuneSockets.Max = RuneSockets.Min;
            }
        }
    }

    internal string GetSocketColors()
    {
        StringBuilder sbColors = new(Resources.Resources.Main210_cbSocketColorsTip);
        sbColors.AppendLine();
        sbColors.Append(Resources.Resources.Main209_cbSocketColors).Append(" : ");
        sbColors.Append(Sockets.RedColor).Append('R').Append(' ');
        sbColors.Append(Sockets.GreenColor).Append('G').Append(' ');
        sbColors.Append(Sockets.BlueColor).Append('B').Append(' ');
        sbColors.Append(Sockets.WhiteColor).Append('W');

        return sbColors.ToString();
    }
}
