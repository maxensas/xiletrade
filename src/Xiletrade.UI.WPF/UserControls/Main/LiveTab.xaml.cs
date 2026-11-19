using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.UserControls.Main;

/// <summary>
/// Tab of a live search : its controls and its last results.
/// </summary>
public partial class LiveTab : UserControl
{
    public LiveTab()
    {
        InitializeComponent();
    }

    // as in the result list : the tooltip of a result is closed as soon as the cursor leaves the result
    private void ResultItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement { ToolTip: ToolTip toolTip })
        {
            toolTip.IsOpen = false;
        }
    }
}
