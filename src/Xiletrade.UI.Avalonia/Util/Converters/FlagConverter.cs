using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Xiletrade.UI.Avalonia.Util.Converters;

public class FlagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int id)
        {
            var app = Application.Current;
            if (app is null)
                return null;

            string resourceKey = id switch
            {
                0 => "FlagEnglish",
                1 => "FlagKorean",
                2 => "FlagFrench",
                3 => "FlagSpanish",
                4 => "FlagGerman",
                5 => "FlagBrazilian",
                6 => "FlagRussian",
                7 => "FlagThai",
                8 => "FlagTaiwanese",
                9 => "FlagChinese",
                10 => "FlagJapanese",
                _ => null
            };

            if (resourceKey is null)
                return null;

            // Cherche la ressource dans le dictionnaire global
            if (app.TryFindResource(resourceKey, out var resource))
                return resource;

            return null;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}