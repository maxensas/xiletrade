using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.Util.Converters;

public sealed class GetSliderRange : IValueConverter
{
    public bool Min { get; set; }
    public int PerCent { get; set; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (PerCent is 0 || value is not double val)
        {
            return value;
        }
        if (val is 0)
        {
            return Min ? -1 : 1;
        }
        
        var negative = val < 0;
        var percent = (val * PerCent) / 100;
        var crement = negative ? -percent : percent;
        crement = crement < 1 ? 1 : crement;
        crement = Min ? -crement : crement;

        return Math.Round(val + crement, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
