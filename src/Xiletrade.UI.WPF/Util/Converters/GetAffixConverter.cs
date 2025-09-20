using System;
using System.Globalization;
using System.Windows.Data;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class GetAffixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AffixFilterEntrie affix)
        {
            var kind = affix.ID.Split('.')[0];
            if (kind is "rune" or "enchant" or "crafted")
            {
                return "craft";
            }
            return kind;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
