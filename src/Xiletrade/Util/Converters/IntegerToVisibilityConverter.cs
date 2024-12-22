using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class IntegerToVisibilityConverter : IValueConverter
{
    public int IntegerValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int @val)
        {
            return @val == IntegerValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
