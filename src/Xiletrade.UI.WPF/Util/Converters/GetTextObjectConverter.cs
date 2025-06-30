using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class GetTextObjectConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && str?.Length > 0)
        {
            var list = new List<string>();
            list.Add(str);
            return list;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> list && list.Count is 1)
        {
            return list[0];
        }
        return value;
    }/*
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TextBox tb && tb.Text.Length > 0)
        {
            var list = new List<string>();
            list.Add(tb.Text);
            return list;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> list && list.Count is 1)
        {
            var tb = new TextBox();
            tb.Text = list[0];
            return tb;
        }
        return value;
    }*/
}
