using Avalonia.Controls;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.Avalonia.Views;

public class ViewBase : Window, IViewBase
{
    public ViewBase() : base()
    {

    }

    public void Center(double scale) => this.Center(scale);
}