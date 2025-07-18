﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class ImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Uri uri)
        {
            return new BitmapImage(uri);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
