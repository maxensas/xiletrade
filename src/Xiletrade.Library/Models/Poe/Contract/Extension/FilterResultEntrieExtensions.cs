using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class FilterResultEntrieExtensions
{
    internal static (FilterResultEntrie Entrie, double Min, double Max) GetMinMaxEntrie(this List<FilterResultEntrie> entries, 
        DataManagerService dm, ItemModifier mod, ItemData item)
    {
        var firstMatches = RegexUtil.DecimalNoPlusPattern().Matches(mod.Parsed);
        var ruleItem = new StatRuleItem(dm, item);

        foreach (var entrie in entries)
        {
            var id = dm.Stat.Resolve(entrie.ID, entrie.Type, ruleItem, out bool skip);
            if (skip)
            {
                continue;
            }

            if (entries.Count > 1 && entrie.Part.Length > 0)
                continue;

            int idxMin = 0, idxMax = 0;
            bool isMin = false, isMax = false, isBreak = true;

            var secondMatches = RegexUtil.DecimalNoPlusDiezePattern().Matches(entrie.Text);
            if (firstMatches.Count == secondMatches.Count)
            {
                for (int t = 0; t < secondMatches.Count; t++)
                {
                    if (secondMatches[t].ValueSpan is "#")
                    {
                        if (!isMin)
                        {
                            isMin = true;
                            idxMin = t;
                        }
                        else if (!isMax)
                        {
                            isMax = true;
                            idxMax = t;
                        }
                    }
                    else if (!firstMatches[t].ValueSpan.SequenceEqual(secondMatches[t].ValueSpan))
                    {
                        isBreak = false;
                        break;
                    }
                }
            }

            if (isBreak) //TOCHECK if does not impact negatively
            {
                var matches = firstMatches; // same input
                if (item.Flag.SanctumRelic) // TO update with other unparsed values not done yet
                {
                    isMin = true;
                }
                var min = isMin && matches.Count > idxMin ? matches[idxMin].Value.ToDoubleEmptyField() : ModFilter.EMPTYFIELD;
                var max = isMax && idxMin < idxMax && matches.Count > idxMax ? matches[idxMax].Value.ToDoubleEmptyField() : ModFilter.EMPTYFIELD;
                return (id == entrie.ID ? entrie : entrie.WithId(id), min, max);
            }
        }
        return (null, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
    }

    // copy carrying the id resolved for the item : the entries of the filters are shared, they are never modified
    private static FilterResultEntrie WithId(this FilterResultEntrie entrie, string id) => new()
    {
        ID = id,
        Text = entrie.Text,
        Type = entrie.Type,
        Part = entrie.Part,
        Option = entrie.Option
    };

    /// <summary>
    /// Item of a price check seen by the stat rules : evaluated on demand, nothing is computed or allocated in advance
    /// (struct used through a generic constraint : no boxing).
    /// </summary>
    private readonly struct StatRuleItem : IStatRuleItem
    {
        private readonly DataManagerService _dm;
        private readonly ItemData _item;

        internal StatRuleItem(DataManagerService dm, ItemData item)
        {
            _dm = dm;
            _item = item;
        }

        public bool HasFlag(StatRuleFlag flag) => flag switch
        {
            StatRuleFlag.Weapon => _item.Flag.Weapon.IsWeapon,
            StatRuleFlag.Armour => _item.Flag.Armour.IsArmour,
            StatRuleFlag.Unique => _item.Flag.Rarity.Unique,
            StatRuleFlag.Jewel => _item.Flag.Jewel.IsJewel,
            StatRuleFlag.Cluster => _item.Flag.Jewel.Cluster,
            StatRuleFlag.Tincture => _item.Flag.Slot.Tincture,
            StatRuleFlag.Charm => _item.Flag.Slot.Charm,
            StatRuleFlag.Flask => _item.Flag.Slot.Flask,
            StatRuleFlag.Amulet => _item.Flag.Jewellery.Amulets,
            StatRuleFlag.Shield => _item.Flag.Offhand.Shield,
            StatRuleFlag.Waystone => _item.Flag.Waystones,
            StatRuleFlag.Tablet => _item.Flag.Tablet,
            StatRuleFlag.Chronicle => _item.Flag.Area.Chronicle,
            StatRuleFlag.Logbook => _item.Flag.Area.Logbook,
            StatRuleFlag.SanctumRelic => _item.Flag.SanctumRelic,
            _ => false // unknown names are rejected when the rules are loaded
        };

        public bool IsUnique(ReadOnlySpan<char> nameEn) => _dm.Words.MatchNameEn(nameEn, _item.UniqueName);

        public bool IsTypeEn(ReadOnlySpan<char> typeEn) => typeEn.SequenceEqual(_item.TypeEn);
    }
}
