using System.Windows;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class NinjaMarginConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string text || text.Length is 0)
        {
            return new Thickness(0, 0, 0, -1);
        }

        double digitCount = text.Length - 1;
        double charWidth = 6;

        double left = 63 + digitCount * charWidth;
        double right = 38 - digitCount * charWidth;

        return new Thickness(left, 0, right, -1);
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}
