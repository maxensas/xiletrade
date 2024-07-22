using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class AffixFlag
{
    private string _parsedData;

    internal bool Crafted { get; set; }
    internal bool Enchant { get; set; }
    internal bool Implicit { get; set; }
    internal bool Fractured { get; set; }
    internal bool Scourged { get; set; }

    internal AffixFlag(string data)
    {
        _parsedData = data;
        Crafted = _parsedData.Contains(Strings.ItemLabel.Crafted, StringComparison.Ordinal);
        // cascade order
        if (Crafted)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Crafted, string.Empty).Trim();
        }
        Enchant = _parsedData.Contains(Strings.ItemLabel.Enchant, StringComparison.Ordinal);
        if (Enchant)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Enchant, string.Empty).Trim();
        }
        Implicit = _parsedData.Contains(Strings.ItemLabel.Implicit, StringComparison.Ordinal);
        if (Implicit)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Implicit, string.Empty).Trim();
        }
        Fractured = _parsedData.Contains(Strings.ItemLabel.Fractured, StringComparison.Ordinal);
        if (Fractured)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Fractured, string.Empty).Trim();
        }
        Scourged = _parsedData.Contains(Strings.ItemLabel.Scourge, StringComparison.Ordinal);
        if (Scourged)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Scourge, string.Empty).Trim();
        }
        /*
        bool augmented = preface[j].Contains("(augmented)", StringComparison.Ordinal);
        if (augmented)
        {
            preface[j] = preface[j].Replace("(augmented)", "").Trim();
        }
        */
    }

    public string ParseAffix(string data)
    {
        return IsOne() ? _parsedData : data;
    }

    private bool IsOne()
    {
        return Crafted || Enchant || Implicit || Fractured || Scourged;
    }
}
