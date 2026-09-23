namespace Xiletrade.Library.Services.Interface.View;

public interface IViewBase
{
    object DataContext { get; set; }

    void Show();
    bool? ShowDialog();
    public void Close();
    public void Center(double scale);
}
