using System;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Currencies of the buyout price of a game : index of the combobox of the form (detail view, custom search)
/// and option of the trade site, index 0 : all currencies (no option sent).
/// </summary>
internal sealed class BuyoutPrice
{
    private readonly string[] _options;

    internal BuyoutPrice(bool isPoe2)
    {
        _options = isPoe2 ? 
            [null, "exalted", "exalted_divine", "chaos", "divine"] : [null, "chaos", "chaos_divine", "divine"];
    }

    /// <summary>Option of the trade site for this index of the form, null : all currencies.</summary>
    internal string GetOption(int index) => index > 0 && index < _options.Length ? _options[index] : null;

    /// <summary>Index of the form for this option of the trade site, 0 : all currencies (none or unknown in this game).</summary>
    internal int GetIndex(string option)
    {
        if (string.IsNullOrEmpty(option))
        {
            return 0;
        }
        var index = Array.IndexOf(_options, option);
        return index > 0 ? index : 0;
    }
}
