namespace Xiletrade.Library.Models.Poe.Domain;

public sealed record SaleInfo
{
    public double Amount { get; }
    public string QualityOrCount { get; }
    public string AgeLabel { get; }
    public string Age { get; }
    public string Account { get; }
    public string HideoutToken { get; }
    public string AccountLabel { get; } = Resources.Resources.Main013_ListName;

    public SaleInfo(double amount, string qualityOrCount, string age, 
        string ageLabel, string account, string hideoutToken)
    {
        Amount = amount;
        QualityOrCount = qualityOrCount;
        Age = age;
        AgeLabel = ageLabel;
        Account = account;
        HideoutToken = hideoutToken;
    }
}
