using Xiletrade.Views;

namespace Xiletrade.Services;

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
