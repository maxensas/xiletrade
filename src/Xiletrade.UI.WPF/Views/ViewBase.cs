using System.Windows;
using Xiletrade.Library.Services.Interface;
using Xiletrade.UI.WPF.Util.Extensions;

namespace Xiletrade.UI.WPF.Views;

public class ViewBase : Window, IViewBase
{
    public ViewBase() : base()
    {

    }

    public void Center(double scale) => this.CenterView(scale);
}
