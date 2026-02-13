using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string icon;

    [ObservableProperty]
    private AsyncObservableCollection<string> socketList;

    [ObservableProperty]
    private int socketColumns;

    [ObservableProperty]
    private int socketRows;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string note;

    [ObservableProperty]
    private bool isVisibleNote;

    [ObservableProperty]
    private ItemRarity rarity;

    [ObservableProperty]
    private bool isCorrupted;

    [ObservableProperty]
    private string corruptedText = Resources.Resources.Main080_lbCorrupted;

    [ObservableProperty]
    private bool isDoubleCorrupted;

    [ObservableProperty]
    private string doubleCorruptedText = Resources.Resources.Main254_twiceCorrupted;

    [ObservableProperty]
    private bool isUnidentified;

    [ObservableProperty]
    private string unidentifiedText = Resources.Resources.General039_Unidentify;

    [ObservableProperty]
    private int itemLevel;

    [ObservableProperty]
    private bool showItemLevel;

    [ObservableProperty]
    private string itemLevelText = Resources.Resources.General032_ItemLv;

    [ObservableProperty]
    private bool isArmourPiece;

    [ObservableProperty]
    private bool isWeaponWithDps;

    [ObservableProperty]
    private AsyncObservableCollection<ItemResultPropertie> propertiesList;

    [ObservableProperty]
    private AsyncObservableCollection<ItemResultPropertie> dpsList;

    [ObservableProperty]
    private AsyncObservableCollection<string> enchantList;

    [ObservableProperty]
    private bool isVisibleEnchant;

    [ObservableProperty]
    private AsyncObservableCollection<string> implicitList;

    [ObservableProperty]
    private bool isVisibleImplicit;

    [ObservableProperty]
    private AsyncObservableCollection<string> runeList;

    [ObservableProperty]
    private bool isVisibleRune;

    [ObservableProperty]
    private AsyncObservableCollection<ItemSkill> grantedSkillList;

    [ObservableProperty]
    private bool isVisibleGrantedSkill;

    [ObservableProperty]
    private AsyncObservableCollection<ItemApi> extendedExplicitList;

    [ObservableProperty]
    private bool isVisibleExplicit;

    [ObservableProperty]
    private bool isVisibleRuneSockets;

    public ResultItemViewModel()
    {

    }

    public ResultItemViewModel(DataManagerService dm, ItemDataApi item)
    {
        /*
        if (item.Rarity is null)
        {
            return;
        }*/
        icon = item.Icon;
        rarity = new(item);
        itemLevel = item.Ilvl;
        showItemLevel = itemLevel > 0;
        isVisibleEnchant = item.EnchantMods?.Length > 0;
        isVisibleImplicit = item.ImplicitMods?.Length > 0;
        isVisibleGrantedSkill = item.GrantedSkills?.Length > 0;
        isVisibleRune = item.Extended?.Hashes.Rune?.Count > 0;
        isCorrupted = item.Corrupted && !item.DoubleCorrupted; // to display only one
        isDoubleCorrupted = item.DoubleCorrupted;
        isUnidentified = !item.Identified;
        isVisibleNote = item.Note?.Length > 0;

        if (isVisibleNote)
        {
            note = item.Note;
        }

        if (item.Sockets?.Length > 0)
        {
            socketList = new();
            foreach (var socket in item.Sockets)
            {
                if (socket.Color?.Length > 0) // poe1 : B,G,R,W
                {
                    socketList.Add(socket.Color);
                }
                if (socket.Type?.Length > 0) // poe2 : rune
                {
                    var kind = socket.Item is null ? "empty" : socket.Item; // socket.Item : rune, soulcore, ...
                    socketList.Add(kind);
                }
            }
            isVisibleRuneSockets = socketList.Count > 0;
            if (isVisibleRuneSockets)
            {
                var cnt = Math.Clamp(socketList.Count, 1, 6);
                socketColumns = cnt is 1 ? 1 : Math.Clamp(item.W, 1, 2);
                socketRows = socketColumns is 1 ? cnt : (cnt + 1) / 2;
            }
        }

        // uncomplete conditional
        var desecrated = item.DesecratedMods?.Length > 0
            && item.Extended?.Hashes.Desecrated?.Count > 0
            && item.Extended.Mods.Desecrated?.Count > 0;
        var crafted = item.CraftedMods?.Length > 0
            && item.Extended?.Hashes.Crafted?.Count > 0
            && item.Extended.Mods.Crafted?.Count > 0;
        var fractured = item.FracturedMods?.Length > 0
            && item.Extended?.Hashes.Fractured?.Count > 0
            && item.Extended.Mods.Fractured?.Count > 0;
        var mutated = item.MutatedMods?.Length > 0;
        isVisibleExplicit = desecrated || fractured || (item.ExplicitMods?.Length > 0 
            && item.Extended?.Hashes.Explicit?.Count > 0 
            && item.Extended.Mods.Explicit?.Count > 0);

        if (item.BaseType?.Length > 0)
        {
            title = (rarity.Unique && item.Name?.Length > 0) ? item.Name : item.BaseType;
        }

        var properties = item.Properties?.Length > 0;
        if (properties)
        {
            propertiesList ??= new();
            foreach (var prop in item.Properties)
            {
                string maxqual = null;
                var ar = prop.Name.StartWith(Strings.ItemApi.Armour);
                var eva = prop.Name.StartWith(Strings.ItemApi.Evasion);
                var es = prop.Name.StartWith(Strings.ItemApi.EnergyShield);
                if (ar || eva || es)
                {
                    isArmourPiece = true;

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
                propertiesList.Add(new(name, val, maxqual));
            }
        }

        var dps = item.Extended?.Dps > 0;
        var pdps = item.Extended?.Pdps > 0;
        var edps = item.Extended?.Edps > 0;

        if (dps || pdps || edps)
        {
            dpsList ??= new();
            isWeaponWithDps = true;
        }
        if (dps)
        {
            dpsList.Add(new(Resources.Resources.Main073_tbTotalDps, item.Extended.Dps));
        }
        if (pdps)
        {
            dpsList.Add(new(Resources.Resources.Main074_tbPhysDps, item.Extended.Pdps));
        }
        if (edps)
        {
            dpsList.Add(new(Resources.Resources.Main075_tbElemDps, item.Extended.Edps));
        }

        if (isVisibleEnchant)
        {
            enchantList = new();
            foreach (var mod in item.EnchantMods)
            {
                enchantList.Add(mod.ArrangeItemInfoDesc());
            }
        }
        if (isVisibleImplicit)
        {
            implicitList = new();
            foreach (var mod in item.ImplicitMods)
            {
                implicitList.Add(mod.ArrangeItemInfoDesc());
            }
        }
        if (isVisibleGrantedSkill)
        {
            grantedSkillList = new();
            foreach (var skill in item.GrantedSkills)
            {
                if (skill.Values is null || skill.Values.Count is 0)
                {
                    break;
                }
                foreach (var value in skill.Values)
                {
                    grantedSkillList.Add(new(skill.Icon, $"{skill.Name}: {value.Item1}"));
                }
            }
        }
        if (IsVisibleRune)
        {
            runeList = new();
            foreach (var map in item.Extended?.Hashes.Rune)
            {
                var stat = dm.Filter.GetFilterDataEntry(map.Id);
                if (stat is not null)
                {
                    runeList.Add(stat.Text);
                }
            }
        }

        if (isVisibleExplicit)
        {
            extendedExplicitList = new();

            if (fractured)
            {
                for (int i = 0; i < item.FracturedMods?.Length; i++)
                {
                    var modId = item.Extended.Hashes.Fractured[i].Values.FirstOrDefault();
                    if (modId >= 0 && modId < item.Extended.Mods.Fractured.Count)
                    {
                        extendedExplicitList.Add(new(item.Extended.Mods.Fractured[modId],
                        item.FracturedMods[i].ArrangeItemInfoDesc(), isFractured: true));
                    }
                }
            }

            for (int i = 0; i < item.ExplicitMods?.Length ; i++)
            {
                //var statId = item.Extended.Hashes.Explicit[i].Id;
                var modId = item.Extended.Hashes.Explicit[i].Values.FirstOrDefault();
                if (modId >= 0 && modId < item.Extended.Mods.Explicit.Count)
                {
                    extendedExplicitList.Add(new(item.Extended.Mods.Explicit[modId],
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
                        extendedExplicitList.Add(new(item.Extended.Mods.Desecrated[modId],
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
                        extendedExplicitList.Add(new(item.Extended.Mods.Explicit[modId],
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
                        extendedExplicitList.Add(new(item.Extended.Mods.Crafted[modId],
                        item.CraftedMods[i].ArrangeItemInfoDesc(), isCrafted: true));
                    }
                }
            }
        }
    }
}
