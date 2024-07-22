using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    private readonly BooleanToVisibilityConverter _converter = new();

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
        return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
        return result != true;
    }
}
