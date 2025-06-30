using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class NullVisibilityConverter : IValueConverter
{
    public bool Collapse { get; set; }
    public bool Reverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool bValue = value != null;

        if (bValue != Reverse)
        {
            return Visibility.Visible;
        }
        else
        {
            return Collapse ? Visibility.Collapsed : Visibility.Hidden;
        }
        //return value == null ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        /*
        bool bValue = value != null;
        Visibility visibility = bValue ? Visibility.Collapsed : Visibility.Hidden;

        return visibility == Visibility.Visible ? !Reverse : Reverse;*/
        throw new NotImplementedException();
    }
}
