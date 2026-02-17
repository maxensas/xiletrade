using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public class ItemCountConverter : IValueConverter
{
    public int IntegerValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection collection && IntegerValue > 0)
        {
            int count = collection.Count;

            return count > IntegerValue;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}