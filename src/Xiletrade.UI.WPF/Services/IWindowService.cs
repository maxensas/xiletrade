using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

public interface IWindowService
{
    public void CreateWindow<T>(object DataContext, bool show) where T : ViewBase, new();
}
