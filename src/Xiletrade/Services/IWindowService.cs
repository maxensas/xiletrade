using Xiletrade.Views;

namespace Xiletrade.Services;

public interface IWindowService
{
    public void CreateWindow<T>(object DataContext, bool show) where T : ViewBase, new();
}
