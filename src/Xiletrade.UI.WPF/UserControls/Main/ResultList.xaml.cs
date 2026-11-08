using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.UserControls.Main;

/// <summary>
/// Logique d'interaction pour ResultList.xaml
/// </summary>
public partial class ResultList : UserControl
{
    public ResultList()
    {
        InitializeComponent();
    }

    // WPF keeps a tooltip open while the cursor moves towards it (safe area between the owner and the tooltip),
    // even outside the result list : the tooltip of a result is closed as soon as the cursor leaves the result.
    private void ResultItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement { ToolTip: ToolTip toolTip })
        {
            toolTip.IsOpen = false;
        }
    }
}
