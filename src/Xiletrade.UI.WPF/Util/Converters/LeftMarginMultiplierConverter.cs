﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xiletrade.UI.WPF.Util.Extensions;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class LeftMarginMultiplierConverter : IValueConverter
{
    public double Length { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var item = value as TreeViewItem;
        if (item == null)
            return new Thickness(0);

        return new Thickness(Length * item.GetDepth(), 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
