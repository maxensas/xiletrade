using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class ItemRarity
{
    public bool Normal { get; }
    public bool Magic { get; }
    public bool Rare { get; }
    public bool Unique { get; }
    public bool UniqueFoil { get; }
    public bool NoRarity { get; }

    public ItemRarity(ItemDataApi item)
    {
        if (item is null || item.Rarity is null)
        {
            NoRarity = item.Rarity is null;
            return;
        }
        Normal = item.Rarity is "Normal";
        Magic = item.Rarity is "Magic";
        Rare = item.Rarity is "Rare";
        Unique = item.Rarity is "Unique";
        UniqueFoil = item.IsRelic;
    }
}
