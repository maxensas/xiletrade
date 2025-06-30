﻿using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class ComboBoxForegroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var comboBoxItemForeground = values[0] as Brush;
        var comboBoxForeground = values[1] as Brush;
        var comboBoxItem = values[2] as ComboBoxItem;

        // Si on est dans un ComboBoxItem, on prend son Foreground, sinon celui du ComboBox
        return comboBoxItem != null ? comboBoxItemForeground : comboBoxForeground;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
