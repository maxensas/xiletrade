using System.Windows;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Util.Extensions;

namespace Xiletrade.Views;

public class ViewBase : Window, IViewBase
{
    public ViewBase() : base()
    {

    }

    public void Center(double scale) => this.CenterView(scale);
}
