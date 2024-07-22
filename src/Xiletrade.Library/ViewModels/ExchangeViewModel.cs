using System;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels;

public sealed class ExchangeViewModel : BaseViewModel
{
    private AsyncObservableCollection<string> category = new();
    private AsyncObservableCollection<string> currency = new();
    private AsyncObservableCollection<string> tier = new();
    private int categoryIndex;
    private int currencyIndex;
    private int tierIndex;
    private Uri image;
    private Uri imageLast;
    private string imageLastToolTip;
    private string imageLastTag;
    private bool tierVisible;
    private bool currencyVisible;
    private string search = string.Empty;

    public AsyncObservableCollection<string> Category { get => category; set => SetProperty(ref category, value); }
    public AsyncObservableCollection<string> Currency { get => currency; set => SetProperty(ref currency, value); }
    public AsyncObservableCollection<string> Tier { get => tier; set => SetProperty(ref tier, value); }
    public int CategoryIndex { get => categoryIndex; set => SetProperty(ref categoryIndex, value); }
    public int CurrencyIndex { get => currencyIndex; set => SetProperty(ref currencyIndex, value); }
    public int TierIndex { get => tierIndex; set => SetProperty(ref tierIndex, value); }
    public Uri Image { get => image; set => SetProperty(ref image, value); }
    public Uri ImageLast { get => imageLast; set => SetProperty(ref imageLast, value); }
    public string ImageLastToolTip { get => imageLastToolTip; set => SetProperty(ref imageLastToolTip, value); }
    public string ImageLastTag { get => imageLastTag; set => SetProperty(ref imageLastTag, value); }
    public bool TierVisible { get => tierVisible; set => SetProperty(ref tierVisible, value); }
    public bool CurrencyVisible { get => currencyVisible; set => SetProperty(ref currencyVisible, value); }
    public string Search { get => search; set => SetProperty(ref search, value); }
}
