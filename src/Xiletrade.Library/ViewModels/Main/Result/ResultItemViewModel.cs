using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private ItemRarity rarity;

    [ObservableProperty]
    private bool isArmourPiece;

    [ObservableProperty]
    private bool isWeaponWithDps;

    [ObservableProperty]
    private AsyncObservableCollection<ItemResultPropertie> propertiesList;

    [ObservableProperty]
    private AsyncObservableCollection<string> enchantList;

    [ObservableProperty]
    private bool isVisibleEnchant;

    [ObservableProperty]
    private AsyncObservableCollection<string> implicitList;

    [ObservableProperty]
    private bool isVisibleImplicit;

    [ObservableProperty]
    private AsyncObservableCollection<ExtendedAffix> extendedExplicitList;

    [ObservableProperty]
    private bool isVisibleExplicit;

    public ResultItemViewModel()
    {

    }

    //TOFINISH
    public ResultItemViewModel(ItemDataApi item)
    {
        rarity = new(item);
        isVisibleEnchant = item.EnchantMods?.Length > 0;
        isVisibleImplicit = item.ImplicitMods?.Length > 0;
        isVisibleExplicit = item.Extended?.Hashes.Explicit?.Count > 0
                && item.Extended.Hashes.Explicit.Count == item.ExplicitMods?.Length
                && item.Extended.Hashes.Explicit.Count == item.Extended.Mods.Explicit?.Count;
        var desecrated = item.Extended?.Hashes.Desecrated?.Count > 0
                && item.Extended.Hashes.Desecrated.Count == item.DesecratedMods?.Length
                && item.Extended.Hashes.Desecrated.Count == item.Extended.Mods.Desecrated?.Count;
        if (item.Rarity is null)
        {
            return;
        }

        if (item.Name is not null && item.BaseType is not null)
        {
            title = (rarity.Unique ? item.Name : item.BaseType);
        }

        var properties = item.Properties?.Length > 0;
        if (properties)
        {
            propertiesList ??= new();
            foreach (var prop in item.Properties)
            {
                decimal? value = null;
                if (prop.Values?.Count > 0 && prop.Values[0].Item1 is not null &&
                    decimal.TryParse(prop.Values[0].Item1, out decimal val))
                {
                    value = val;
                }
                if (prop.Name.StartWith(Strings.ItemApi.Armour)
                    || prop.Name.StartWith(Strings.ItemApi.Evasion)
                    || prop.Name.StartWith(Strings.ItemApi.EnergyShield))
                {
                    isArmourPiece = true;
                }
                propertiesList.Add(new(prop.Name.ArrangeItemInfoDesc(), value));
            }
        }

        var dps = item.Extended?.Dps > 0;
        var pdps = item.Extended?.Pdps > 0;
        var edps = item.Extended?.Edps > 0;

        if (dps || pdps || edps)
        {
            propertiesList ??= new();
            isWeaponWithDps = true;
        }
        if (dps)
        {
            propertiesList.Add(new(Resources.Resources.Main073_tbTotalDps, item.Extended.Dps));
        }
        if (pdps)
        {
            propertiesList.Add(new(Resources.Resources.Main074_tbPhysDps, item.Extended.Pdps));
        }
        if (edps)
        {
            propertiesList.Add(new(Resources.Resources.Main075_tbElemDps, item.Extended.Edps));
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
        if (isVisibleExplicit)
        {
            extendedExplicitList = new();

            for (int i = 0; i < item.ExplicitMods?.Length 
                && i < item.Extended.Hashes.Explicit?.Count; i++)
            {
                var idx = item.Extended.Hashes.Explicit[i].Values.FirstOrDefault();
                if (idx >= 0 && idx < item.Extended.Mods.Explicit.Count)
                {
                    var ext = item.Extended.Mods.Explicit[idx];
                    ext.Mod = item.ExplicitMods[i].ArrangeItemInfoDesc();
                    UpdateExtendedTag(ext);
                    extendedExplicitList.Add(ext);
                }
            }

            if (desecrated)
            {
                for (int i = 0; i < item.DesecratedMods?.Length
                && i < item.Extended.Hashes.Desecrated?.Count; i++)
                {
                    var idx = item.Extended.Hashes.Explicit[i].Values.FirstOrDefault();
                    if (idx >= 0 && idx < item.Extended.Mods.Desecrated.Count)
                    {
                        var ext = item.Extended.Mods.Desecrated[idx];
                        ext.Mod = item.DesecratedMods[i].ArrangeItemInfoDesc();
                        ext.TagDesecrated = true;
                        extendedExplicitList.Add(ext);
                    }
                }
            }
        }
    }

    private static void UpdateExtendedTag(ExtendedAffix ext)
    {
        if (ext.Magnitudes.Count is 1)
        {
            var id = ext.Magnitudes[0].Hash;
            ext.TagLife = id is Strings.Stat.MaxLife;
            ext.TagFire = id is Strings.Stat.FireResist;
            ext.TagCold = id is Strings.Stat.ColdResist;
            ext.TagLightning = id is Strings.Stat.LightningResist;
        }
    }
}
