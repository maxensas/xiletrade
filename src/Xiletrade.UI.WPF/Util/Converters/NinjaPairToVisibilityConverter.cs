using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;

namespace Xiletrade.UI.WPF.Util.Converters;

public class NinjaPairToVisibilityConverter : IValueConverter
{
    public bool Reverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is NinjaPair { History.Count: > 0 } dataPair &&
            dataPair.History.Min(d => d.Timestamp.DateTime) < dataPair.History.Max(d => d.Timestamp.DateTime);

        if (Reverse)
            isVisible = !isVisible;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
