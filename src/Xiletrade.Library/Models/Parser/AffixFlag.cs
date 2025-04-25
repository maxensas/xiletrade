using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

internal sealed class AffixFlag
{
    private string _parsedData;

    internal bool Crafted { get; set; }
    internal bool Enchant { get; set; }
    internal bool Implicit { get; set; }
    internal bool Fractured { get; set; }
    internal bool Scourged { get; set; }
    internal bool Augmented { get; set; }
    internal bool Rune { get; set; }

    internal AffixFlag(string data)
    {
        _parsedData = data;
        Crafted = _parsedData.Contain(Strings.ItemLabel.Crafted);
        // cascade order
        if (Crafted)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Crafted, string.Empty).Trim();
        }
        Enchant = _parsedData.Contain(Strings.ItemLabel.Enchant);
        if (Enchant)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Enchant, string.Empty).Trim();
        }
        Implicit = _parsedData.Contain(Strings.ItemLabel.Implicit);
        if (Implicit)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Implicit, string.Empty).Trim();
        }
        Fractured = _parsedData.Contain(Strings.ItemLabel.Fractured);
        if (Fractured)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Fractured, string.Empty).Trim();
        }
        Scourged = _parsedData.Contain(Strings.ItemLabel.Scourge);
        if (Scourged)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Scourge, string.Empty).Trim();
        }
        Augmented = _parsedData.Contain(Strings.ItemLabel.Augmented);
        if (Augmented)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Augmented, string.Empty)
                .Replace("%", string.Empty).Replace("+", string.Empty).Trim();
        }
        Rune = _parsedData.Contain(Strings.ItemLabel.Rune);
        if (Rune)
        {
            _parsedData = _parsedData.Replace(Strings.ItemLabel.Rune, string.Empty).Trim();
        }
    }

    public string ParseAffix(string data)
    {
        return IsOne() ? _parsedData : data;
    }

    private bool IsOne()
    {
        return Crafted || Enchant || Implicit || Fractured || Scourged || Augmented || Rune;
    }
}
