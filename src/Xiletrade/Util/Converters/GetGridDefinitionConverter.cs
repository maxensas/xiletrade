using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class GetGridDefinitionConverter : IValueConverter
{
    public bool Row { get; set; }
    public bool First { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Row)
        {
            return value is bool valRow && valRow ? 43 : 0; // Row MaxHeight
        }
        if (First)
        {
            return value is bool valFirstCol && valFirstCol ? 0 : 14; // First Col MaxWidth
        }
        return value is bool valLastCol && valLastCol ? 100 : 86; // Last Col MinWidth
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
