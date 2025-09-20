using System;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class StartWithConverter : IValueConverter
{
    public string StringValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null && (value as string).StartsWith(StringValue, StringComparison.Ordinal);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
