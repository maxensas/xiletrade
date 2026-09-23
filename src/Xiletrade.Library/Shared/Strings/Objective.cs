using System;

namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Objective
    {
        internal const string Moderate = "moderate";
        internal const string High = "high";
        internal const string Precious = "precious";
        internal const string Priceless = "priceless";

        internal static string GetObjectiveValue(ReadOnlySpan<char> objective)
        {
            return objective.SequenceEqual(Resources.Resources.General233_ModerateValue) ? Moderate
                : objective.SequenceEqual(Resources.Resources.General234_HighValue) ? High
                : objective.SequenceEqual(Resources.Resources.General235_Precious) ? Precious
                : objective.SequenceEqual(Resources.Resources.General236_Priceless) ? Priceless
                : string.Empty;
        }
    }
}
