using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class NinjaWidthConverter : IValueConverter
{
    public bool IsTextBlockWidth { get; set; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string valueString && valueString.Length > 0)
        {
            double nbDigit = valueString.Length - 1;
            double charLength = 6;
            if (IsTextBlockWidth)
            {
                return 76 + nbDigit * charLength;
            }
            return 90 + nbDigit * charLength; // btn
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
