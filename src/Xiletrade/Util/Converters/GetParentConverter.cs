using System;
using System.Globalization;

namespace Xiletrade.Util.Converters;

public sealed class GetParentConverter : System.Windows.Data.IMultiValueConverter
{
    public GetParentConverter()
    {
        System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
    }
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (object val in values)
        {
            if (val is string)
            {
                return val;
            }
        }
        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
