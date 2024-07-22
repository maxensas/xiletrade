using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class HotkeyViewModel : BaseViewModel
{
    private bool isEnable;
    private string hotkey = string.Empty;
    private string val = string.Empty;
    private AsyncObservableCollection<string> list;
    private int listIndex;

    public bool IsEnable { get => isEnable; set => SetProperty(ref isEnable, value); }
    public string Hotkey { get => hotkey; set => SetProperty(ref hotkey, value); }
    public string Val { get => val; set => SetProperty(ref val, value); }
    public AsyncObservableCollection<string> List { get => list; set => SetProperty(ref list, value); }
    public int ListIndex { get => listIndex; set => SetProperty(ref listIndex, value); }
}
