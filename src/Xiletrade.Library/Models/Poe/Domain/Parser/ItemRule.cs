using Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ItemRule
{
    internal bool ByType { get; }
    internal bool ByBase { get; }
    internal bool ShowDetail { get; }
    internal bool Parseable { get; }

    /// <summary>
    /// Item parsing behaviors and rules
    /// </summary>
    /// <param name="flag"></param>
    internal ItemRule(ItemFlag flag)
    {
        var noArea = !flag.Area.Chronicle && !flag.Invitation && !flag.MercenaryWarrant && (!flag.Area.Ultimatum || flag.UltimatumPoe2);

        ShowDetail = flag.Gem.IsGem && !flag.Tag.Imbued || flag.Divcard || flag.AllflameEmber || flag.Breachstone
            || flag.Map.Misc && noArea || flag.Tag.CapturedBeast
            || flag.Map.Fragment && !flag.Area.MirroredTablet && noArea
            || flag.Currency && !flag.Area.MirroredTablet && noArea;

        Parseable = !(ShowDetail && !flag.Facetor && !flag.UltimatumPoe2 && !flag.Corpses && !flag.Wombgift && !flag.ScryingOrb
            && !flag.Gem.IsGem && !flag.Tag.Imbued && !flag.Area.SanctumResearch && !flag.Area.TrialCoins);

        ByType = flag.Jewellery.IsJewellery || flag.Socket.CanSocket;

        ByBase = flag.Rarity.Unique || flag.Rarity.Normal || flag.Gem.IsGem
            || flag.Map.IsMap || flag.Map.Chart || flag.Area.Logbook
            || flag.Tag.CapturedBeast || flag.Tag.Unidentified
            || flag.Slot.Flask || flag.Slot.Tincture || flag.Slot.Charm
            || flag.Currency || flag.Waystones || flag.Divcard
            || flag.Invitation || flag.Tablet || flag.Graft;
    }
}
