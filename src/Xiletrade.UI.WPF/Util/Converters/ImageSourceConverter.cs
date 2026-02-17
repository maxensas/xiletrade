using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Xiletrade.UI.WPF.Util.Converters;

public sealed class ImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Uri uri = null;

        if (value is Uri u)
            uri = u;
        else if (value is string s && s.StartsWith("/gen/image/"))
            uri = new Uri("https://web.poecdn.com" + s);

        if (uri != null)
        {
            var bmp = new BitmapImage();

            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.EndInit();

            if (bmp.CanFreeze)
                bmp.Freeze();

            return bmp;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
