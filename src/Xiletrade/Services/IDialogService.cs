using Xiletrade.Views;

namespace Xiletrade.Services;

public interface IDialogService
{
    public void CreateDialog<T>(object DataContext) where T : ViewBase, new();
}
