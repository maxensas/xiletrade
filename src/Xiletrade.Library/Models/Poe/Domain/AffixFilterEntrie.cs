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
        IsImplicitCorruption = useDesc && affix.Description.IsImplicitCorruption && entrie.ID.StartWith(Strings.Type.Implicit);
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
        var cult = CultureInfo.InvariantCulture;
        return affix == rm.GetString(Strings.Resource.Enchant, cult) ? Resources.Resources.General011_Enchant
            : affix == rm.GetString(Strings.Resource.Crafted, cult) ? Resources.Resources.General012_Crafted
            : affix == rm.GetString(Strings.Resource.Implicit, cult) ? Resources.Resources.General013_Implicit
            : affix == rm.GetString(Strings.Resource.Pseudo, cult) ? Resources.Resources.General014_Pseudo
            : affix == rm.GetString(Strings.Resource.Explicit, cult) ? Resources.Resources.General015_Explicit
            : affix == rm.GetString(Strings.Resource.Fractured, cult) ? Resources.Resources.General016_Fractured
            : affix == rm.GetString(Strings.Resource.Monster, cult) ? Resources.Resources.General018_Monster
            : affix == rm.GetString(Strings.Resource.Scourge, cult) ? Resources.Resources.General099_Scourge
            : affix == rm.GetString(Strings.Resource.Desecrated, cult) ? Resources.Resources.General158_Desecrated
            : affix;
    }
}