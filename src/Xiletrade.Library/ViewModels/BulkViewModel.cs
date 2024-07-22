namespace Xiletrade.Library.ViewModels;

public sealed class BulkViewModel : BaseViewModel
{
    private bool autoSelect;
    private string args;
    private string currency;
    private string tier;
    private string stock = "1";
    private ExchangeViewModel get = new();
    private ExchangeViewModel pay = new();

    public bool AutoSelect { get => autoSelect; set => SetProperty(ref autoSelect, value); }
    public string Args { get => args; set => SetProperty(ref args, value); }
    public string Currency { get => currency; set => SetProperty(ref currency, value); }
    public string Tier { get => tier; set => SetProperty(ref tier, value); }
    public ExchangeViewModel Get { get => get; set => SetProperty(ref get, value); }
    public ExchangeViewModel Pay { get => pay; set => SetProperty(ref pay, value); }
    public string Stock { get => stock; set => SetProperty(ref stock, value); }
}
