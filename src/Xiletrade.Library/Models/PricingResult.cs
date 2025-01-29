using System;
using System.Net.Http;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class PricingResult
{
    internal string FirstLine { get; set; } = Resources.Resources.Main007_PriceWaiting;
    internal string SecondLine { get; set; } = string.Empty;
    internal int Min { get; set; } = -1;
    internal int Max { get; set; } = -1;
    internal string CurrencyMin { get; set; } = string.Empty;
    internal string CurrencyMax { get; set; } = string.Empty;

    internal PricingResult()
    {

    }

    internal PricingResult(string first, string second)
    {
        FirstLine = first;
        SecondLine = second;
    }

    internal void SetNoResult()
    {
        FirstLine = Resources.Resources.Main008_PriceNoResult;
        SecondLine = "NORESULT";
    }

    internal void SetErrorBadLeague()
    {
        FirstLine = Resources.Resources.Main028_Error1;
        SecondLine = "ERROR " + Resources.Resources.Main029_Error1bis;
    }

    internal void SetErrorNoData()
    {
        FirstLine = Resources.Resources.Main030_Error2;
        SecondLine = "ERROR " + Resources.Resources.Main031_Error2bis; //"ERROR contacting trade website.";
    }

    internal void SetHttpException(HttpRequestException exception)
    {
        string[] mess = exception.Message.Split(':');
        FirstLine = "The request encountered" + Strings.LF + "an exception. [A]";
        SecondLine = mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message;
    }

    internal void SetTimeoutException(TimeoutException exception)
    {
        FirstLine = "The request has expired";
        SecondLine = exception.Message.Length > 24 ? exception.Message[..24].Trim() 
            + Strings.LF + exception.Message[24..].Trim() : exception.Message;
    }
}
