using System;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Xiletrade.UI.WPF.Util.Extensions;

public class ImageAssetExtension : MarkupExtension
{
    private static readonly string pack = "pack://application:,,,/Xiletrade;component/Assets/Png/";

    public string Path { get; set; }

    public ImageAssetExtension() { }

    public ImageAssetExtension(string path)
    {
        Path = path;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Path))
            return null;

        var bmp = new BitmapImage();

        bmp.BeginInit();
        bmp.UriSource = new Uri($"{pack}{Path}", UriKind.Absolute);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.None;
        bmp.EndInit();

        if (bmp.CanFreeze)
            bmp.Freeze();

        return bmp;
    }
}
