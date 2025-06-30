using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class MultiplyConverter : IValueConverter
{
    public int Coefficient { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isDouble = value is double;
        if (isDouble)
        {
            double val = (double)value;
            return val * Coefficient; // Math.Truncate(val * Coefficient);
        }
        else
        {
            return value;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isDouble = value is double;
        if (isDouble)
        {
            double val = (double)value;
            return val / Coefficient;
        }
        else
        {
            return value;
        }
    }
}
