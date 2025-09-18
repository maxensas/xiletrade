namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class OfferItem
{
    internal string Message { get; set; }
    internal double Ammount { get; set; }

    internal OfferItem(string message, double ammount)
    {
        Message = message;
        Ammount = ammount;
    }
}
