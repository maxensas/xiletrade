using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class GreaterThanConverter : IValueConverter
{
    public int MaxValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int integer)
        {
            return integer > MaxValue;
        }
        if (value is ICollection collection)
        {
            return collection.Count > MaxValue;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
