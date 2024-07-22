using System.Text.RegularExpressions;

namespace Xiletrade.Library.Shared;

// https://regexland.com/regex-decimal-numbers/
/// <summary>Static class containing all regex patterns.</summary>
internal static partial class RegexUtil
{
    /*
    private static readonly Regex numerical = new("[^0-9.-]+");
    private static readonly Regex letter = new("[^a-zA-Z]");
    private static readonly Regex alphaNumerical = new("^[a-zA-Z0-9_]*$"); // @"^[A-Z]+[a-zA-Z""'\s-]*$"

    public static readonly string decimalPattern = @"[+-]?[0-9]+\.[0-9]+|[+-]?[0-9]+";
    public static readonly string decimalPatternNoPlus = @"[-]?[0-9]+\.[0-9]+|[-]?[0-9]+";
    public static readonly string decimalPatternNoPlusDieze = @"[-]?[0-9]+\.[0-9]+|[-]?[0-9]+|#";

    public static bool IsNumeric(string text)
    {
        return !numerical.IsMatch(text);
    }
    public static bool IsLetter(string text)
    {
        return !letter.IsMatch(text);
    }
    public static bool IsAlphaNumeric(string text)
    {
        return !alphaNumerical.IsMatch(text);
    }
    */
    internal static readonly string DecimalPatternDieze = @"[+-]?([0-9]+\.[0-9]+|[0-9]+|\#)";

    [GeneratedRegex(@"\(.*?\)")]
    internal static partial Regex BetweenBracketsPattern();

    [GeneratedRegex(@"\\#")]
    internal static partial Regex DiezePattern();

    [GeneratedRegex("# ")]
    internal static partial Regex DiezeSpacePattern();

    [GeneratedRegex(@"\([a-zA-Z]+\)")]
    internal static partial Regex LetterPattern();

    [GeneratedRegex(@"(timeless-)?([a-z]{3})[a-z\-]+\-([a-z]+)")]
    internal static partial Regex LetterTimelessPattern();

    [GeneratedRegex(@"[+-]?[0-9]+\.[0-9]+|[+-]?[0-9]+")]
    internal static partial Regex DecimalPattern();

    [GeneratedRegex(@"[-]?[0-9]+\.[0-9]+|[-]?[0-9]+")]
    internal static partial Regex DecimalNoPlusPattern();

    [GeneratedRegex(@"[-]?[0-9]+\.[0-9]+|[-]?[0-9]+|#")]
    internal static partial Regex DecimalNoPlusDiezePattern();

    [GeneratedRegex("[^0-9]")]
    internal static partial Regex NumericalPattern();

    [GeneratedRegex("[^0-9.]")]
    internal static partial Regex NumericalPattern2();

    [GeneratedRegex("<(uniqueitem|prophecy|divination|gemitem|magicitem|rareitem|whiteitem|corrupted|default|normal|augmented|size:[0-9]+)>")]
    internal static partial Regex DetailPattern();

    [GeneratedRegex(@"^pseudo.pseudo_adds_[a-z]+_damage$")]
    internal static partial Regex AddsDamagePattern();
}
