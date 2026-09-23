namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Net
    {
        internal static readonly string UserAgent = "OAuth Xiletrade/" + Common.GetFileVersion() + " (contact: xiletrade@gmail.com)";

        internal const string XrateLimitPolicy = "X-Rate-Limit-Policy";
        internal const string TradeSearchRequestLimit = "trade-search-request-limit";
        internal const string TradeFetchRequestLimit = "trade-fetch-request-limit";
        internal const string TradeExchangeRequestLimit = "trade-exchange-request-limit";
        internal const string RetryAfter = "Retry-After";

        internal static readonly (string Rule, string State)[] XRateRules =
        [
            ("X-Rate-Limit-Ip", "X-Rate-Limit-Ip-State"),
            ("X-Rate-Limit-Account", "X-Rate-Limit-Account-State"),
            ("X-Rate-Limit-Client", "X-Rate-Limit-Client-State")
        ];
    }
}
