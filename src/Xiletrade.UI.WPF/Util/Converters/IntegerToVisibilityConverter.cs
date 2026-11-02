using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class IntegerToVisibilityConverter : IValueConverter
{
    public int IntegerValue { get; set; }

    /// <summary>True : visible when the value is different from <see cref="IntegerValue"/>.</summary>
    public bool Reverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int @val)
        {
            return (@val == IntegerValue) != Reverse ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
