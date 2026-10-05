using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Xiletrade.Library.Views;

namespace Xiletrade.UI.Avalonia.UserControls;

public partial class TaskbarIcon : UserControl, ITaskbar
{
    public TaskbarIcon(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}