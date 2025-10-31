using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal abstract record NinjaInfoBase
{
    internal readonly DataManagerService _dm;
    internal readonly PoeNinjaService _ninja;

    internal string League { get; set; }
    internal string Type { get; set; }
    internal string Url { get; set; }
    internal string Link { get; set; }
    internal bool VerifiedLink { get; set; }

    internal NinjaInfoBase(DataManagerService dm, PoeNinjaService ninja)
    {
        _dm = dm;
        _ninja = ninja;
    }
}
