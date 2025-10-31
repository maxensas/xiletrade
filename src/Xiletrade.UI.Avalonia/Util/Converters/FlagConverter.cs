using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.Avalonia.Util.Converters;

public class FlagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Lang id)
        {
            var app = Application.Current;
            if (app is null)
                return null;

            string resourceKey = id switch
            {
                Lang.English => "FlagEnglish",
                Lang.Korean => "FlagKorean",
                Lang.French => "FlagFrench",
                Lang.Spanish => "FlagSpanish",
                Lang.German => "FlagGerman",
                Lang.Portuguese => "FlagBrazilian",
                Lang.Russian => "FlagRussian",
                Lang.Thai => "FlagThai",
                Lang.Taiwanese => "FlagTaiwanese",
                Lang.Chinese => "FlagChinese",
                Lang.Japanese => "FlagJapanese",
                _ => null
            };

            if (resourceKey is null)
                return null;

            // Search for the resource in the global dictionary
            if (app.TryFindResource(resourceKey, out var resource))
                return resource;

            return null;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}