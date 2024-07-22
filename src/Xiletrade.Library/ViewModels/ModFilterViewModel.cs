namespace Xiletrade.Library.ViewModels;

public sealed class ModFilterViewModel : BaseViewModel
{
    private int num;
    private string id;
    private string type;
    private string text;

    public int Num { get => num; set => SetProperty(ref num, value); }
    public string Id { get => id; set => SetProperty(ref id, value); }
    public string Type { get => type; set => SetProperty(ref type, value); }
    public string Text { get => text; set => SetProperty(ref text, value); }
}
