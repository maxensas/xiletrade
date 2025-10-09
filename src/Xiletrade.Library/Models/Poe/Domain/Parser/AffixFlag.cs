using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class AffixFlag
{
    internal string ParsedData { get; }

    internal bool Crafted { get; private set; }
    internal bool Enchant { get; private set; }
    internal bool Implicit { get; private set; }
    internal bool Fractured { get; private set; }
    internal bool Scourged { get; private set; }
    internal bool Augmented { get; private set; }
    internal bool Rune { get; private set; }
    internal bool Desecrated { get; private set; }

    internal AffixFlag(ReadOnlySpan<char> data)
    {
        var crafted = Strings.ItemLabel.Crafted.AsSpan();
        if (data.Contain(crafted))
        {
            Crafted = true;
            ParsedData = RemoveLabel(data, crafted);
            return;
        }
        var enchant = Strings.ItemLabel.Enchant.AsSpan();
        if (data.Contain(enchant))
        {
            Enchant = true;
            ParsedData = RemoveLabel(data, enchant);
            return;
        }
        var impli = Strings.ItemLabel.Implicit.AsSpan();
        if (data.Contain(impli))
        {
            Implicit = true;
            ParsedData = RemoveLabel(data, impli);
            return;
        }
        var fractured = Strings.ItemLabel.Fractured.AsSpan();
        if (data.Contain(fractured))
        {
            Fractured = true;
            ParsedData = RemoveLabel(data, fractured);
            return;
        }
        var scourge = Strings.ItemLabel.Scourge.AsSpan();
        if (data.Contain(scourge))
        {
            Scourged = true;
            ParsedData = RemoveLabel(data, scourge);
            return;
        }
        var augmented = Strings.ItemLabel.Augmented.AsSpan();
        if (data.Contain(augmented))
        {
            Augmented = true;
            ParsedData = RemoveLabel(data, augmented).Replace("%", "").Replace("+", "").Trim(); // not optimized
            return;
        }
        var rune = Strings.ItemLabel.Rune.AsSpan();
        if (data.Contain(rune))
        {
            Rune = true;
            ParsedData = RemoveLabel(data, rune);
            return;
        }
        var desecrated = Strings.ItemLabel.Desecrated.AsSpan();
        if (data.Contain(desecrated))
        {
            Desecrated = true;
            ParsedData = RemoveLabel(data, desecrated);
            return;
        }
        ParsedData = data.ToString();
    }

    private static string RemoveLabel(ReadOnlySpan<char> data, ReadOnlySpan<char> label)
    {
        var index = data.IdxOf(label);
        if (index < 0) return data.ToString();

        var before = data[..index];
        var after = data[(index + label.Length)..];

        return string.Concat(before, after).Trim();
    }
}
