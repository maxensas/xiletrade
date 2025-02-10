using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class PricingCurrency
{
    internal double Amount { get; set; } = -1;
    internal string Label { get; set; } = string.Empty;
    internal Uri Uri { get; set; } = null;

    internal PricingCurrency(string label, double amount)
    {
        if (label?.Length > 0)
        {
            Label = label;
            Uri = Common.GetCurrencyImageUri(label);
        }
        if (amount > 0)
        {
            Amount = amount;
        }
    }
}
