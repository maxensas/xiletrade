namespace Xiletrade.Library.Models;

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
