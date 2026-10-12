using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Status
    {
        //market
        internal const string Available = "available"; // instant buy out & person trade
        internal const string Online = "online"; // person trade only
        internal const string Securable = "securable"; // instant buy out only

        //person
        internal const string Afk = "afk";
        internal const string Offline = "offline";

        internal static string GetColorStatus(TradeStatus status, bool isBulkTheme = false)
        {
            if (isBulkTheme)
            {
                return status is TradeStatus.Error ? Color.DeepSkyBlue
                : status is TradeStatus.Afk ? Color.Yellow
                : status is TradeStatus.Offline ? Color.DarkRed
                : Color.Red;
            }
            return status is TradeStatus.Async ? Color.White
                : status is TradeStatus.Online ? Color.LimeGreen
                : status is TradeStatus.Afk ? Color.YellowGreen
                : status is TradeStatus.Offline ? Color.Red
                : Color.Red;
        }
    }
}
