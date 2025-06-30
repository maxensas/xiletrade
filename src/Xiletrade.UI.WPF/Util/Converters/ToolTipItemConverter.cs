using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Xiletrade.Library.Models;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class ToolTipItemConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<ToolTipItem> listTip)
        {
            var tip = listTip.Where(x => x.Kind is "Life" or "Fire" or "Cold" or "Lightning");
            if (tip.Any())
            {
                return tip.First().Kind;
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
