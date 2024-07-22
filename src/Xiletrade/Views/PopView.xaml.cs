using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xiletrade.Library.Shared;

namespace Xiletrade.Views;

/// <summary>Open a popup image centered on the screen.</summary>
/// <remarks>by using a WPF window (non-MvvM)</remarks>
public partial class PopView : ViewBase
{
    private string JpgPath { get; set; }

    public PopView(string jpgName)
    {
        InitializeComponent();
        Loaded += Window_Loaded;
        Closing += Window_Closing;
        Closed += Window_Closed;
        imgJpg.MouseDown += Image_MouseDown;

        Name = Strings.WindowName.Popup;
        string path = System.IO.Path.GetFullPath("Data\\");
        JpgPath = path + jpgName;

        this.Show();
    }

    //events
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (System.IO.File.Exists(JpgPath))
        {
            var uri = new Uri(JpgPath);
            var bitmap = new BitmapImage();
            using var stream = System.IO.File.OpenRead(uri.AbsolutePath);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            imgJpg.Source = ConvertBitmapToDPI(bitmap, 128); // def is 96
            stream.Dispose();
            stream.Close();

            this.Left = (SystemParameters.PrimaryScreenWidth - imgJpg.Source.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - imgJpg.Source.Height) / 2;
            return;
        }
        this.Close();
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // https://github.com/dotnet/wpf/issues/2397
        // usefull trick to solve memory leak
        imgJpg.Source = null;
        imgJpg.UpdateLayout();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        GC.Collect();
    }

    //methods
    private static BitmapSource ConvertBitmapToDPI(BitmapImage bitmapImage, int dpi)
    {
        int width = bitmapImage.PixelWidth;
        int height = bitmapImage.PixelHeight;

        int stride = width * bitmapImage.Format.BitsPerPixel;
        byte[] pixelData = new byte[stride * height];
        bitmapImage.CopyPixels(pixelData, stride, 0);

        return BitmapSource.Create(width, height, dpi, dpi, bitmapImage.Format, null, pixelData, stride);
    }
}
