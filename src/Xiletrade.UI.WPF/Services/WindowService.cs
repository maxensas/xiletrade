using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

internal class WindowService : IWindowService
{
    public void CreateWindow<T>(object DataContext, bool show) where T : ViewBase, new()
    {
        T window = new()
        {
            DataContext = DataContext
        };
        if (show)
        {
            window.Show();
        }
    }
}
