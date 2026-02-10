using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public class ItemCountToRowsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection collection)
        {
            int count = collection.Count;

            if (count <= 2) return 1;
            if (count <= 4) return 2;
            if (count <= 6) return 3;
        }

        return 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}