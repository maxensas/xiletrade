using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public class ItemNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return ExtractBetweenParentheses(str) ?? str;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static string ExtractBetweenParentheses(ReadOnlySpan<char> value)
    {
        int start = value.IndexOf('(');
        if (start < 0)
            return null;

        ReadOnlySpan<char> remaining = value[(start + 1)..];

        int end = remaining.IndexOf(')');
        if (end < 0)
            return null;

        return remaining[..end].ToString();
    }
}
