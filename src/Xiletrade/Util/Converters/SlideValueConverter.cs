using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class SlideValueConverter : IValueConverter
{
    public bool Min { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int valInt)
        {
            bool negative = valInt < 0;
            var twentyPerCent = ((valInt / 100) * 20);
            var crement = negative ? - twentyPerCent : twentyPerCent;
            if (Min)
            {
                return valInt - crement;
            }
            return valInt + crement;
        }
        if (value is double val)
        {
            bool negative = val < 0;
            var twentyPerCent = ((val / 100) * 20);
            var crement = negative ? - twentyPerCent : twentyPerCent;
            if (Min)
            {
                return Math.Round(val - crement, 0);
            }
            return Math.Round(val + crement, 0);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
