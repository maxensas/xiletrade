using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class ImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Uri uri = null;
        bool isLocal = false;

        if (value is Uri u)
            uri = u;
        else if (value is CurrencyInfo curInfo)
        {
            isLocal = TryGetImageUri(curInfo, out string imageUri);
            if (isLocal)
            {
                uri = new Uri($"{imageUri}", UriKind.Absolute);
            }
            else
            {
                value = curInfo.Uri;
            }
        }
        if (uri is null && value is string s && s.StartsWith("/gen/image/"))
            uri = new Uri("https://web.poecdn.com" + s);

        if (uri is null)
        {
           return value; 
        }

        var bmp = new BitmapImage();

        bmp.BeginInit();
        bmp.UriSource = uri;
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = isLocal ? BitmapCreateOptions.None 
            : BitmapCreateOptions.IgnoreImageCache;
        bmp.EndInit();

        if (bmp.CanFreeze)
            bmp.Freeze();

        return bmp;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static bool TryGetImageUri(CurrencyInfo curInfo, out string imageUri)
    {
        imageUri = string.Empty;

        var isChaos = curInfo.Name is Strings.TradeCurrency.Chaos;
        var isExalt = curInfo.Name is Strings.TradeCurrency.Exalted;
        var isDivine = curInfo.Name is Strings.TradeCurrency.Divine;

        var isOne = isChaos || isExalt || isDivine;
        if (!isOne)
        {
            return false;
        }

        var resource = isChaos ? curInfo.IsPoe2 ? Application.Current.Resources["ImgChaosPoe2"]
                : Application.Current.Resources["ImgChaos"]
                : isExalt ? curInfo.IsPoe2 ? Application.Current.Resources["ImgExaltPoe2"]
                : Application.Current.Resources["ImgExalt"]
                : isDivine ? curInfo.IsPoe2 ? Application.Current.Resources["ImgDivinePoe2"]
                : Application.Current.Resources["ImgDivine"] : null;

        if (resource is string uri)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                imageUri = uri;
                return true;
            }
        }
        return false;
    }
}
