using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

public interface IWindowService
{
    public void CreateWindow<T>(object DataContext, bool show) where T : IViewBase;

    public Task CreateDialog<T>(object DataContext) where T : IViewBase;
}
