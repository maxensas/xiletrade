using System;
using System.ComponentModel;
using Avalonia.Input;

namespace Xiletrade.UI.Avalonia.Models;

public sealed class AvaloniaKeyConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string str && Enum.TryParse(typeof(Key), str, true, out var result))
        {
            return result;
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Key key)
        {
            return key.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
