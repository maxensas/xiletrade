using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xiletrade.Library.Shared;

namespace Xiletrade.UI.WPF.Views;

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
            var bmp = new BitmapImage();

            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.EndInit();

            if (bmp.CanFreeze)
                bmp.Freeze();

            imgJpg.Source = bmp;

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
        Loaded -= Window_Loaded;
        Closing -= Window_Closing;
        Closed -= Window_Closed;
        imgJpg.MouseDown -= Image_MouseDown;

        Content = null;

        Common.CollectGarbage();
    }
}
