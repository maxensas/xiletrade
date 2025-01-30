using System;
using System.Collections.Generic;
using System.Net.Http;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal enum PricingResultSate
{
    Init, Fetched, NoResult, Error
}

internal sealed class PricingResult
{
    internal PricingResultSate State { get; private set; } = PricingResultSate.Init;
    internal string FirstLine { get; private set; } = Resources.Resources.Main007_PriceWaiting;
    internal string SecondLine { get;  private set; } = string.Empty;
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

    internal void Fetch(Dictionary<string, int> currencys)
    {
        if (currencys.Count is 0) // Only first time : && Global.StatsFetchDetail[4] < total
        {
            return;
        }

        List<KeyValuePair<string, int>> myList = new(currencys);
        string first = myList[0].Key;
        string last = myList[^1].Key; // myList.Count - 1

        myList.Sort(
            delegate (KeyValuePair<string, int> firstPair,
            KeyValuePair<string, int> nextPair)
            {
                return -1 * firstPair.Value.CompareTo(nextPair.Value);
            }
        );

        KeyValuePair<string, int> firstKey = myList[^1]; // myList.Count - 1
        if (myList.Count > 1 && (firstKey.Value is 1 || firstKey.Value is 2 && first == firstKey.Key))
        {
            int idx = myList.Count - 2;

            if (firstKey.Value is 1 || myList[idx].Value is 1)
                idx = (int)Math.Truncate((double)myList.Count / 2);

            firstKey = myList[idx];
        }
        bool isMany = first != last;
        string concatPrice = isMany ? first + " (" + Resources.Resources.Main022_ResultsMin + ")"
            + Strings.LF + last + " (" + Resources.Resources.Main023_ResultsMax + ")"
            : first + Strings.LF + Resources.Resources.Main141_ResultsSingle; // single price 
        var curMin = first.Split(' ');
        if (curMin.Length is 2 && int.TryParse(curMin[0], out int min))
        {
            Min = min;
            CurrencyMin = curMin[1];
        }
        if (isMany)
        {
            var curMax = last.Split(' ');
            if (curMax.Length is 2 && int.TryParse(curMax[0], out int max))
            {
                Max = max;
                CurrencyMax = curMax[1];
            }
        }
        FirstLine = RegexUtil.LetterTimelessPattern().Replace(concatPrice, @"$3`$2");

        int lineCount = 0, records = 0;
        for (int i = 0; i < myList.Count; i++)
        {
            if (myList[i].Value >= 2 && lineCount < 3)
            {
                if (lineCount > 0) SecondLine += Strings.LF;
                SecondLine += myList[i].Key + " : " + myList[i].Value + " " + Resources.Resources.Main024_ResultsSales; // sales
                lineCount++;
            }
            records += myList[i].Value;
        }
        SecondLine = RegexUtil.LetterTimelessPattern().Replace(SecondLine.TrimEnd(',', ' '), @"$3`$2");
        if (SecondLine.Length is 0 && records < 10) SecondLine = Resources.Resources.Main012_PriceFew;

        State = PricingResultSate.Fetched;
    }

    internal void SetNoResult()
    {
        FirstLine = Resources.Resources.Main008_PriceNoResult;
        SecondLine = "NORESULT";
        State = PricingResultSate.NoResult;
    }

    internal void SetErrorBadLeague()
    {
        FirstLine = Resources.Resources.Main028_Error1;
        SecondLine = "ERROR " + Resources.Resources.Main029_Error1bis;
        State = PricingResultSate.Error;
    }

    internal void SetErrorNoData()
    {
        FirstLine = Resources.Resources.Main030_Error2;
        SecondLine = "ERROR " + Resources.Resources.Main031_Error2bis; //"ERROR contacting trade website.";
        State = PricingResultSate.Error;
    }

    internal void SetHttpException(HttpRequestException exception)
    {
        string[] mess = exception.Message.Split(':');
        FirstLine = "The request encountered" + Strings.LF + "an exception. [A]";
        SecondLine = mess.Length > 1 ? "ERROR : Code " + mess[1].Trim() : exception.Message;
        State = PricingResultSate.Error;
    }

    internal void SetTimeoutException(TimeoutException exception)
    {
        FirstLine = "The request has expired";
        SecondLine = exception.Message.Length > 24 ? exception.Message[..24].Trim() 
            + Strings.LF + exception.Message[24..].Trim() : exception.Message;
        State = PricingResultSate.Error;
    }
}
