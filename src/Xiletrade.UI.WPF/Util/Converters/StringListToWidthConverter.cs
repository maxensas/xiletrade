using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class StringListToWidthConverter : IValueConverter
{
    public double PixelsPerCharacter { get; set; } // approximation value
    public double ExtraWidth { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> items)
        {
            int maxLength = items.Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.Length).DefaultIfEmpty(0).Max();

            return maxLength * PixelsPerCharacter + ExtraWidth;
        }
        if (value is IEnumerable<UniqueItem> unique)
        {
            int maxLength = unique.Where(s => !string.IsNullOrEmpty(s.Name))
                .Select(s => s.Name.Length).DefaultIfEmpty(0).Max();

            return maxLength * PixelsPerCharacter + ExtraWidth;
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}