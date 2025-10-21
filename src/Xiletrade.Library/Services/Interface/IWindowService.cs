namespace Xiletrade.Library.Services.Interface;

public interface IWindowService
{
    public void CreateWindow<T>(object DataContext, bool show) where T : IViewBase;

    public void CreateDialog<T>(object DataContext) where T : IViewBase;
}
