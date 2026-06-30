using System.Globalization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed record AffixFilterEntrie
{
    public string ID { get; }
    public string Name { get; }
    public string Type { get; }

    public bool IsImplicitRegular { get; }
    public bool IsImplicitEnch { get; }
    public bool IsImplicitScourge { get; }
    public bool IsImplicitAugment { get; }
    public bool IsImplicitCorruption { get; }

    public bool IsExplicitUnique { get; }
    public bool IsExplicitMutated { get; }
    public bool IsExplicitCrafted { get; }

    internal AffixFilterEntrie(string id, string name)
    {
        ID = id;
        Name = name;
        Type = id.Split('.')[0];
    }

    internal AffixFilterEntrie(DataManagerService dm, FilterResult filter, FilterResultEntrie entrie, ItemData item, AffixFlag affix)
    {
        ID = entrie.ID;
        Name = dm.Config.Options.Language > 0 ? GetTranslatedAffix(filter.Label) : filter.Label;
        Type = entrie.Type?.Length > 0 ? entrie.Type : entrie.ID.Split('.')[0];

        //implicits
        var useDesc = affix.Description is not null;
        IsImplicitCorruption = useDesc && affix.Description.IsCorruption && entrie.ID.StartWith(Strings.Type.Implicit) && !item.IsPoe2;
        IsImplicitRegular = Name == Resources.Resources.General013_Implicit;
        IsImplicitEnch = Name == Resources.Resources.General011_Enchant;
        IsImplicitScourge = Name == Resources.Resources.General099_Scourge;
        IsImplicitAugment = Name == Resources.Resources.General145_Augment;

        //explicits
        IsExplicitUnique = item.Flag.Unique && entrie.ID.StartWith(Strings.Type.Explicit);
        bool isFoulborn = useDesc && affix.Description.IsAffixUniqueFoulborn && entrie.ID.StartWith(Strings.Type.Explicit);
        bool isVaal = useDesc && affix.Description.IsAffixUniqueVaal && entrie.ID.StartWith(Strings.Type.Explicit);
        IsExplicitMutated = affix.Mutated || isFoulborn || isVaal;
        IsExplicitCrafted = Name == Resources.Resources.General012_Crafted;
    }

    private static string GetTranslatedAffix(string affix)
    {
        var rm = Resources.Resources.ResourceManager;
        return affix == rm.GetEnglish(nameof(Resources.Resources.General011_Enchant)) ? Resources.Resources.General011_Enchant
            : affix == rm.GetEnglish(nameof(Resources.Resources.General012_Crafted)) ? Resources.Resources.General012_Crafted
            : affix == rm.GetEnglish(nameof(Resources.Resources.General013_Implicit)) ? Resources.Resources.General013_Implicit
            : affix == rm.GetEnglish(nameof(Resources.Resources.General014_Pseudo)) ? Resources.Resources.General014_Pseudo
            : affix == rm.GetEnglish(nameof(Resources.Resources.General015_Explicit)) ? Resources.Resources.General015_Explicit
            : affix == rm.GetEnglish(nameof(Resources.Resources.General016_Fractured)) ? Resources.Resources.General016_Fractured
            : affix == rm.GetEnglish(nameof(Resources.Resources.General018_Monster)) ? Resources.Resources.General018_Monster
            : affix == rm.GetEnglish(nameof(Resources.Resources.General099_Scourge)) ? Resources.Resources.General099_Scourge
            : affix == rm.GetEnglish(nameof(Resources.Resources.General158_Desecrated)) ? Resources.Resources.General158_Desecrated
            : affix;
    }
}