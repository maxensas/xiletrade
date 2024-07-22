using System;
using System.Windows.Input;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed class OfferViewModel : BaseViewModel
{
    private string getMessage;
    private string payMessage;
    private double sellerAmount;
    private string sellerCurrency;
    private double buyerAmount;
    private string buyerCurrency;
    private double sellerStock;
    private double getAmount;
    private double payAmount;
    private Uri imageGet;
    private Uri imagePay;
    private bool sliderVisible;
    private string labelPay;
    private string labelGet;
    private double minimumValue;

    public string GetMessage { get => getMessage; set => SetProperty(ref getMessage, value); }
    public string PayMessage { get => payMessage; set => SetProperty(ref payMessage, value); }
    public double SellerAmount { get => sellerAmount; set => SetProperty(ref sellerAmount, value); }
    public string SellerCurrency { get => sellerCurrency; set => SetProperty(ref sellerCurrency, value); }
    public double BuyerAmount { get => buyerAmount; set => SetProperty(ref buyerAmount, value); }
    public string BuyerCurrency { get => buyerCurrency; set => SetProperty(ref buyerCurrency, value); }
    public double SellerStock { get => sellerStock; set => SetProperty(ref sellerStock, value); }
    public double GetAmount { get => getAmount; set => SetProperty(ref getAmount, value); }
    public double PayAmount { get => payAmount; set => SetProperty(ref payAmount, value); }
    public Uri ImageGet { get => imageGet; set => SetProperty(ref imageGet, value); }
    public Uri ImagePay { get => imagePay; set => SetProperty(ref imagePay, value); }
    public bool SliderVisible { get => sliderVisible; set => SetProperty(ref sliderVisible, value); }
    public string LabelPay { get => labelPay; set => SetProperty(ref labelPay, value); }
    public string LabelGet { get => labelGet; set => SetProperty(ref labelGet, value); }
    public double MinimumValue { get => minimumValue; set => SetProperty(ref minimumValue, value); }

    private readonly DelegateCommand slideValueChange;
    public ICommand SlideValueChange => slideValueChange;
    
    public OfferViewModel()
    {
        slideValueChange = new(OnSlideValueChange, CanSlideValueChange);
    }

    private bool CanSlideValueChange(object commandParameter)
    {
        return true;
    }

    private void OnSlideValueChange(object commandParameter)
    {
        PayAmount = Math.Round(GetAmount / SellerAmount * BuyerAmount, 2, MidpointRounding.ToEven);
    }
}
