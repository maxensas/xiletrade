using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Xiletrade.Library.ViewModels;

public sealed partial class WhisperOfferViewModel : ViewModelBase
{
    [ObservableProperty]
    private string getMessage;

    [ObservableProperty]
    private string payMessage;

    [ObservableProperty]
    private double sellerAmount;

    [ObservableProperty]
    private string sellerCurrency;

    [ObservableProperty]
    private double buyerAmount;

    [ObservableProperty]
    private string buyerCurrency;

    [ObservableProperty]
    private double sellerStock;

    [ObservableProperty]
    private double getAmount;

    [ObservableProperty]
    private double payAmount;

    [ObservableProperty]
    private Uri imageGet;

    [ObservableProperty]
    private Uri imagePay;

    [ObservableProperty]
    private bool sliderVisible;

    [ObservableProperty]
    private string labelPay;

    [ObservableProperty]
    private string labelGet;

    [ObservableProperty]
    private double minimumValue;

    [RelayCommand]
    private void SlideValueChange(object commandParameter)
    {
        PayAmount = Math.Round(GetAmount / SellerAmount * BuyerAmount, 2, MidpointRounding.ToEven);
    }
}
