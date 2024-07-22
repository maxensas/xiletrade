namespace Xiletrade.Library.ViewModels;

public sealed class ListItemViewModel : BaseViewModel
{
    private string content;
    private string toolTip;
    private string tag;
    private int index;
    private string fgColor;

    public string Content { get => content; set => SetProperty(ref content, value); }
    public string ToolTip { get => toolTip; set => SetProperty(ref toolTip, value); }
    public string Tag { get => tag; set => SetProperty(ref tag, value); }
    public int Index { get => index; set => SetProperty(ref index, value); }
    public string FgColor { get => fgColor; set => SetProperty(ref fgColor, value); }
}
