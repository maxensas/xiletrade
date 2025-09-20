using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class AffixFlag
{
    internal string ParsedData { get; }

    internal bool Crafted { get; }
    internal bool Enchant { get; }
    internal bool Implicit { get; }
    internal bool Fractured { get; }
    internal bool Scourged { get; }
    internal bool Augmented { get; }
    internal bool Rune { get; }
    internal bool Desecrated { get; }

    internal AffixFlag(string data)
    {
        ParsedData = data;
        Crafted = ParsedData.Contain(Strings.ItemLabel.Crafted);
        // cascade order
        if (Crafted)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Crafted, string.Empty).Trim();
        }
        Enchant = ParsedData.Contain(Strings.ItemLabel.Enchant);
        if (Enchant)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Enchant, string.Empty).Trim();
        }
        Implicit = ParsedData.Contain(Strings.ItemLabel.Implicit);
        if (Implicit)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Implicit, string.Empty).Trim();
        }
        Fractured = ParsedData.Contain(Strings.ItemLabel.Fractured);
        if (Fractured)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Fractured, string.Empty).Trim();
        }
        Scourged = ParsedData.Contain(Strings.ItemLabel.Scourge);
        if (Scourged)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Scourge, string.Empty).Trim();
        }
        Augmented = ParsedData.Contain(Strings.ItemLabel.Augmented);
        if (Augmented)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Augmented, string.Empty)
                .Replace("%", string.Empty).Replace("+", string.Empty).Trim();
        }
        Rune = ParsedData.Contain(Strings.ItemLabel.Rune);
        if (Rune)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Rune, string.Empty).Trim();
        }
        Desecrated = ParsedData.Contain(Strings.ItemLabel.Desecrated);
        if (Desecrated)
        {
            ParsedData = ParsedData.Replace(Strings.ItemLabel.Desecrated, string.Empty).Trim();
        }
    }

    internal string ParseAffix(string data)
    {
        return IsOne() ? ParsedData : data;
    }

    private bool IsOne()
    {
        return Crafted || Enchant || Implicit || Fractured || Scourged || Augmented || Rune || Desecrated;
    }
}
