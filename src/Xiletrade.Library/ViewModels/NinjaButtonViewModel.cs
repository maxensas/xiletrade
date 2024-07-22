namespace Xiletrade.Library.ViewModels;

public sealed class NinjaButtonViewModel : BaseViewModel
{
    private bool visible;
    private string price;
    private double valWidth = 0;
    private double btnWidth = 0;
    private string imageName;
    private string imgLeftRightMargin;
    //private Thickness imgMargin;

    public bool Visible { get => visible; set => SetProperty(ref visible, value); }
    public string Price { get => price; set => SetProperty(ref price, value); }
    public double ValWidth { get => valWidth; set => SetProperty(ref valWidth, value); }
    public double BtnWidth { get => btnWidth; set => SetProperty(ref btnWidth, value); }
    public string ImageName { get => imageName; set => SetProperty(ref imageName, value); }
    public string ImgLeftRightMargin { get => imgLeftRightMargin; set => SetProperty(ref imgLeftRightMargin, value); }
    //public Thickness ImgMargin { get => imgMargin; set => SetProperty(ref imgMargin, value); }
}
