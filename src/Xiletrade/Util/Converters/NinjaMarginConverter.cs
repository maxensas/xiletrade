using System.Windows;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class NinjaMarginConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var str = System.Convert.ToString(value)?.Split('.');
        if(str.Length is 2)
        {
            return new Thickness(System.Convert.ToDouble(str[0]), 0, System.Convert.ToDouble(str[1]), -1);
        }
        return new Thickness(0, 0, 0, -1);
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}
