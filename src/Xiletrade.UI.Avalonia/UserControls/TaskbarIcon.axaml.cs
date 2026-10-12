using Avalonia;
using Avalonia.Controls;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.UserControls;

public partial class TaskbarIcon : UserControl, ITaskbar
{
    private readonly TrayIcons _icons; // keep reference

    public TaskbarIcon()
    {
        InitializeComponent();
    }

    public TaskbarIcon(object vm) : this()
    {
        DataContext = vm;

        if (Content is TrayIcons icons)
        {
            _icons = icons;
            Content = null; // Detaches the TrayIcons from the UserControl
            TrayIcon.SetIcons(Application.Current!, icons);
        }
    }
}