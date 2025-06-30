using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

internal class DialogService : IDialogService
{
    public void CreateDialog<T>(object DataContext) where T : ViewBase, new()
    {
        T window = new()
        {
            DataContext = DataContext
        };
        window.ShowDialog();
    }
}
