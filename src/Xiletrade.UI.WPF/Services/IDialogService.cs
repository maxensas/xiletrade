using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

public interface IDialogService
{
    public void CreateDialog<T>(object DataContext) where T : ViewBase, new();
}
