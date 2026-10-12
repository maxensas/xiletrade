namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Emoji
    {
        internal const string VeryHappy = "emoji_vhappy";
        internal const string Happy = "emoji_happy";
        internal const string Neutral = "emoji_neutral";
        internal const string Crying = "emoji_crying";
        internal const string Angry = "emoji_angry";

        internal static string GetNinjaTag(double ratio)
        {
            return ratio >= 1.2 ? VeryHappy
                : ratio >= 1 ? Happy
                : ratio >= 0.90 ? Neutral
                : ratio >= 0.80 ? Crying
                : Angry;
        }
    }
}
