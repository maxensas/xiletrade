using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed record SaleItem
{
    public string Icon { get; }
    public string Title { get; }
    public string Note { get; }
    public string ItemLevelText { get; } = Resources.Resources.General032_ItemLv;
    public string CorruptedText { get; } = Resources.Resources.Main080_lbCorrupted;
    public string DoubleCorruptedText { get; } = Resources.Resources.Main254_twiceCorrupted;
    public string UnidentifiedText { get; } = Resources.Resources.General039_Unidentify;

    public int SocketColumns { get; }
    public int SocketRows { get; }
    public int ItemLevel { get; }

    public bool ShowItemLevel { get; }
    public bool IsVisibleNote { get; }
    public bool IsCorrupted { get; }
    public bool IsDoubleCorrupted { get; }
    public bool IsUnidentified { get; }
    public bool IsArmourPiece { get; }
    public bool IsWeaponWithDps { get; }
    public bool IsVisibleEnchant { get; }
    public bool IsVisibleImplicit { get; }
    public bool IsVisibleRune { get; }
    public bool IsVisibleGrantedSkill { get; }
    public bool IsVisibleExplicit { get; }
    public bool IsVisibleRuneSockets { get; }    

    public IReadOnlyList<string> SocketList { get; }
    public IReadOnlyList<string> EnchantList { get; }
    public IReadOnlyList<string> ImplicitList { get; }
    public IReadOnlyList<string> RuneList { get; }
    public IReadOnlyList<ItemResultPropertie> PropertiesList { get; }
    public IReadOnlyList<ItemResultPropertie> DpsList { get; }
    public IReadOnlyList<ItemSkill> GrantedSkillList { get; }
    public IReadOnlyList<ItemApi> ExtendedExplicitList { get; }

    public ItemRarity Rarity { get; }

    public SaleItem(DataManagerService dm, ItemDataApi item)
    {
        Icon = item.Icon;
        Rarity = new(item);
        ItemLevel = item.Ilvl;
        ShowItemLevel = ItemLevel > 0;
        IsVisibleEnchant = item.EnchantMods?.Length > 0;
        IsVisibleImplicit = item.ImplicitMods?.Length > 0;
        IsVisibleGrantedSkill = item.GrantedSkills?.Length > 0;
        IsVisibleRune = item.Extended?.Hashes?.Rune?.Count > 0;
        IsCorrupted = item.Corrupted && !item.DoubleCorrupted; // to display only one
        IsDoubleCorrupted = item.DoubleCorrupted;
        IsUnidentified = !item.Identified;
        IsVisibleNote = item.Note?.Length > 0;

        if (IsVisibleNote)
        {
            Note = item.Note;
        }

        if (item.Sockets?.Length > 0)
        {
            var sList = new List<string>();
            //socketList = new();
            foreach (var socket in item.Sockets)
            {
                if (socket.Color?.Length > 0) // poe1 : B,G,R,W
                {
                    sList.Add(socket.Color);
                }
                if (socket.Type?.Length > 0) // poe2 : rune
                {
                    var kind = socket.Item is null ? "empty" : socket.Item; // socket.Item : rune, soulcore, ...
                    sList.Add(kind);
                }
            }
            SocketList = sList;
            IsVisibleRuneSockets = SocketList.Count > 0;
            if (IsVisibleRuneSockets)
            {
                var cnt = Math.Clamp(SocketList.Count, 1, 6);
                SocketColumns = cnt is 1 ? 1 : Math.Clamp(item.W, 1, 2);
                SocketRows = SocketColumns is 1 ? cnt : (cnt + 1) / 2;
            }
        }

        // uncomplete conditional
        var desecrated = item.DesecratedMods?.Length > 0
            && item.Extended?.Hashes?.Desecrated?.Count > 0
            && item.Extended?.Mods?.Desecrated?.Count > 0;
        var crafted = item.CraftedMods?.Length > 0
            && item.Extended?.Hashes?.Crafted?.Count > 0
            && item.Extended?.Mods?.Crafted?.Count > 0;
        var fractured = item.FracturedMods?.Length > 0
            && item.Extended?.Hashes?.Fractured?.Count > 0
            && item.Extended?.Mods?.Fractured?.Count > 0;
        var mutated = item.MutatedMods?.Length > 0;
        IsVisibleExplicit = desecrated || fractured || item.ExplicitMods?.Length > 0
            && item.Extended?.Hashes?.Explicit?.Count > 0
            && item.Extended?.Mods?.Explicit?.Count > 0;

        if (item.BaseType?.Length > 0)
        {
            Title = Rarity.Unique && item.Name?.Length > 0 ? item.Name : item.BaseType;
        }

        var properties = item.Properties?.Length > 0;
        if (properties)
        {
            var lProp = new List<ItemResultPropertie>();
            foreach (var prop in item.Properties)
            {
                string maxqual = null;
                var ar = prop.Name.Contain(Strings.ItemApi.Armour);
                var eva = prop.Name.Contain(Strings.ItemApi.Evasion);
                var es = prop.Name.Contain(Strings.ItemApi.EnergyShield);
                if (ar || eva || es)
                {
                    IsArmourPiece = true;

                    if (ar && item.Extended.ArMaxDisplay && item.Extended.ArMaxQuality > 0)
                    {
                        maxqual = item.Extended.ArMaxQuality.ToString();
                    }
                    if (eva && item.Extended.EvaMaxDisplay && item.Extended.EvaMaxQuality > 0)
                    {
                        maxqual = item.Extended.EvaMaxQuality.ToString();
                    }
                    if (es && item.Extended.EsMaxDisplay && item.Extended.EsMaxQuality > 0)
                    {
                        maxqual = item.Extended.EsMaxQuality.ToString();
                    }
                }
                var name = prop.Name.ArrangeItemInfoDesc();
                var isStrFormat = name.Contain("{0}");
                var val = prop.Values.Count >= 1 && !isStrFormat ? prop.Values[0].Item1 : null;
                if (isStrFormat)
                {
                    var values = prop.Values.Select(x => x.Item1).ToArray();
                    name = string.Format(name, values);
                }
                lProp.Add(new(name, val, maxqual));
            }
            PropertiesList = lProp;
        }

        var dps = item.Extended?.Dps > 0;
        var pdps = item.Extended?.Pdps > 0;
        var edps = item.Extended?.Edps > 0;

        var lDps = new List<ItemResultPropertie>();
        if (dps)
        {
            lDps.Add(new(Resources.Resources.Main073_tbTotalDps, item.Extended.Dps));
        }
        if (pdps)
        {
            lDps.Add(new(Resources.Resources.Main074_tbPhysDps, item.Extended.Pdps));
        }
        if (edps)
        {
            lDps.Add(new(Resources.Resources.Main075_tbElemDps, item.Extended.Edps));
        }
        if (dps || pdps || edps)
        {
            DpsList = lDps;
            IsWeaponWithDps = true;
        }

        if (IsVisibleEnchant)
        {
            var lEnch = new List<string>();
            foreach (var mod in item.EnchantMods)
            {
                lEnch.Add(mod.ArrangeItemInfoDesc());
            }
            EnchantList = lEnch;
        }
        if (IsVisibleImplicit)
        {
            var lImp = new List<string>();
            foreach (var mod in item.ImplicitMods)
            {
                lImp.Add(mod.ArrangeItemInfoDesc());
            }
            ImplicitList = lImp;
        }
        if (IsVisibleGrantedSkill)
        {
            var lSkill = new List<ItemSkill>();
            foreach (var skill in item.GrantedSkills)
            {
                if (skill.Values is null || skill.Values.Count is 0)
                {
                    break;
                }
                foreach (var value in skill.Values)
                {
                    lSkill.Add(new(skill.Icon, $"{skill.Name}: {value.Item1}"));
                }
            }
            GrantedSkillList = lSkill;
        }
        if (IsVisibleRune)
        {
            var lImp = new List<string>();
            foreach (var map in item.Extended?.Hashes.Rune)
            {
                var stat = dm.Filter.GetFilterDataEntry(map.Id);
                if (stat is not null)
                {
                    lImp.Add(stat.Text);
                }
            }
            RuneList = lImp;
        }

        if (IsVisibleExplicit)
        {
            var lExplicit = new List<ItemApi>();
            if (fractured)
            {
                for (int i = 0; i < item.FracturedMods?.Length; i++)
                {
                    var modId = item.Extended.Hashes.Fractured[i].Values.FirstOrDefault();
                    if (modId >= 0 && modId < item.Extended.Mods.Fractured.Count)
                    {
                        lExplicit.Add(new(item.Extended.Mods.Fractured[modId],
                        item.FracturedMods[i].ArrangeItemInfoDesc(), isFractured: true));
                    }
                }
            }

            for (int i = 0; i < item.ExplicitMods?.Length; i++)
            {
                //var statId = item.Extended.Hashes.Explicit[i].Id;
                var modId = item.Extended.Hashes.Explicit[i].Values.FirstOrDefault();
                if (modId >= 0 && modId < item.Extended.Mods.Explicit.Count)
                {
                    lExplicit.Add(new(item.Extended.Mods.Explicit[modId],
                        item.ExplicitMods[i].ArrangeItemInfoDesc()));
                }
            }

            if (desecrated)
            {
                for (int i = 0; i < item.DesecratedMods?.Length; i++)
                {
                    var modId = item.Extended.Hashes.Desecrated[i].Values.FirstOrDefault();
                    if (modId >= 0 && modId < item.Extended.Mods.Desecrated.Count)
                    {
                        lExplicit.Add(new(item.Extended.Mods.Desecrated[modId],
                        item.DesecratedMods[i].ArrangeItemInfoDesc(), isDesecrated: true));
                    }
                }
            }

            // GGG will probably update this behaviour if mutated mods will remain in POE.
            if (mutated)
            {
                var nbExplicit = item.ExplicitMods.Length;
                // mutated are the latests from explicit list
                var mapMutated = item.Extended.Hashes.Explicit.Skip(nbExplicit).ToList();
                //var modsMutated = item.Extended.Mods.Explicit.Skip(nbExplicit).ToList();

                if (mapMutated.Count is 0 || mapMutated.Count != item.MutatedMods?.Length)
                {
                    return;
                }

                for (int i = 0; i < item.MutatedMods?.Length; i++)
                {
                    var modId = mapMutated[i].Values.FirstOrDefault();

                    if (modId >= 0 && modId < item.Extended.Mods.Explicit.Count)
                    {
                        lExplicit.Add(new(item.Extended.Mods.Explicit[modId],
                        item.MutatedMods[i].ArrangeItemInfoDesc(), isMutated: true));
                    }
                }
            }

            if (crafted)
            {
                for (int i = 0; i < item.CraftedMods?.Length; i++)
                {
                    var modId = item.Extended.Hashes.Crafted[i].Values.FirstOrDefault();
                    if (modId >= 0 && modId < item.Extended.Mods.Crafted.Count)
                    {
                        lExplicit.Add(new(item.Extended.Mods.Crafted[modId],
                        item.CraftedMods[i].ArrangeItemInfoDesc(), isCrafted: true));
                    }
                }
            }

            ExtendedExplicitList = lExplicit;
        }
    }
}
