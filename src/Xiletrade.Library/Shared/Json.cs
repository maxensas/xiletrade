using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utf8Json;
using Utf8Json.Resolvers;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Shared;

/// <summary>Static helper class used for JSON serialization.</summary>
/// <remarks>Utf8Json used for serialization. </remarks>
internal static class Json
{
    private static IServiceProvider _serviceProvider;

    internal static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal static string Serialize<T>(object obj) where T : class
    {
        return JsonSerializer.ToJsonString((T)obj, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    internal static T Deserialize<T>(string strData) where T : class
    {
        byte[] data = Encoding.UTF8.GetBytes(strData);
        return JsonSerializer.Deserialize<T>(data, StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    internal static string GetSerialized(XiletradeItem itemOptions, ItemBaseName currentItem, bool useSaleType, string market)
    {
        static string BeforeDayToString(int day)
        {
            if (day < 3) return "1day";
            if (day < 7) return "3days";
            if (day < 14) return "1week";
            return "2weeks";
        }

        try
        {
            // TODO: move to Model business logic.
            JsonData jsonData = new();
            Query JQ = jsonData.Query;
            Options optTrue = new("true"), optFalse = new("false");

            string Inherit = currentItem.Inherits.Length > 0 ? currentItem.Inherits[0] : string.Empty;
            string Inherit2 = currentItem.Inherits.Length > 1 ? currentItem.Inherits[1] : string.Empty;

            JQ.Stats = []; // Array.Empty<Stats>();

            if (itemOptions.ChkArmour || itemOptions.ChkEnergy || itemOptions.ChkEvasion || itemOptions.ChkWard)
            {
                if (Modifier.IsNotEmpty(itemOptions.ArmourMin))
                    JQ.Filters.Armour.Filters.Armour.Min = itemOptions.ArmourMin;
                if (Modifier.IsNotEmpty(itemOptions.ArmourMax))
                    JQ.Filters.Armour.Filters.Armour.Max = itemOptions.ArmourMax;
                if (Modifier.IsNotEmpty(itemOptions.EnergyMin))
                    JQ.Filters.Armour.Filters.Energy.Min = itemOptions.EnergyMin;
                if (Modifier.IsNotEmpty(itemOptions.EnergyMax))
                    JQ.Filters.Armour.Filters.Energy.Max = itemOptions.EnergyMax;
                if (Modifier.IsNotEmpty(itemOptions.EvasionMin))
                    JQ.Filters.Armour.Filters.Evasion.Min = itemOptions.EvasionMin;
                if (Modifier.IsNotEmpty(itemOptions.EvasionMax))
                    JQ.Filters.Armour.Filters.Evasion.Max = itemOptions.EvasionMax;
                if (Modifier.IsNotEmpty(itemOptions.WardMin))
                    JQ.Filters.Armour.Filters.Ward.Min = itemOptions.WardMin;
                if (Modifier.IsNotEmpty(itemOptions.WardMax))
                    JQ.Filters.Armour.Filters.Ward.Max = itemOptions.WardMax;

                JQ.Filters.Armour.Disabled = false;
            }
            else
            {
                JQ.Filters.Armour.Disabled = true;
            }

            if (itemOptions.ChkDpsTotal || itemOptions.ChkDpsPhys || itemOptions.ChkDpsElem)
            {
                if (itemOptions.ChkDpsTotal)
                {
                    if (Modifier.IsNotEmpty(itemOptions.DpsTotalMin))
                        JQ.Filters.Weapon.Filters.Damage.Min = itemOptions.DpsTotalMin;
                    if (Modifier.IsNotEmpty(itemOptions.DpsTotalMax))
                        JQ.Filters.Weapon.Filters.Damage.Max = itemOptions.DpsTotalMax;
                }
                if (itemOptions.ChkDpsPhys)
                {
                    if (Modifier.IsNotEmpty(itemOptions.DpsPhysMin))
                        JQ.Filters.Weapon.Filters.Pdps.Min = itemOptions.DpsPhysMin;
                    if (Modifier.IsNotEmpty(itemOptions.DpsPhysMax))
                        JQ.Filters.Weapon.Filters.Pdps.Max = itemOptions.DpsPhysMax;
                }
                if (itemOptions.ChkDpsElem)
                {
                    if (Modifier.IsNotEmpty(itemOptions.DpsElemMin))
                        JQ.Filters.Weapon.Filters.Edps.Min = itemOptions.DpsElemMin;
                    if (Modifier.IsNotEmpty(itemOptions.DpsElemMax))
                        JQ.Filters.Weapon.Filters.Edps.Max = itemOptions.DpsElemMax;
                }

                JQ.Filters.Weapon.Disabled = false;
            }
            else
            {
                JQ.Filters.Weapon.Disabled = true;
            }

            if (itemOptions.ChkResolve || itemOptions.ChkMaxResolve || itemOptions.ChkInspiration || itemOptions.ChkAureus)
            {
                if (itemOptions.ChkResolve)
                {
                    if (Modifier.IsNotEmpty(itemOptions.ResolveMin))
                        JQ.Filters.Sanctum.Filters.Resolve.Min = itemOptions.ResolveMin;
                    if (Modifier.IsNotEmpty(itemOptions.ResolveMax))
                        JQ.Filters.Sanctum.Filters.Resolve.Max = itemOptions.ResolveMax;
                }

                if (itemOptions.ChkMaxResolve)
                {
                    if (Modifier.IsNotEmpty(itemOptions.MaxResolveMin))
                        JQ.Filters.Sanctum.Filters.MaxResolve.Min = itemOptions.MaxResolveMin;
                    if (Modifier.IsNotEmpty(itemOptions.MaxResolveMax))
                        JQ.Filters.Sanctum.Filters.MaxResolve.Max = itemOptions.MaxResolveMax;
                }

                if (itemOptions.ChkInspiration)
                {
                    if (Modifier.IsNotEmpty(itemOptions.InspirationMin))
                        JQ.Filters.Sanctum.Filters.Inspiration.Min = itemOptions.InspirationMin;
                    if (Modifier.IsNotEmpty(itemOptions.InspirationMax))
                        JQ.Filters.Sanctum.Filters.Inspiration.Max = itemOptions.InspirationMax;
                }

                if (itemOptions.ChkAureus)
                {
                    if (Modifier.IsNotEmpty(itemOptions.AureusMin))
                        JQ.Filters.Sanctum.Filters.Aureus.Min = itemOptions.AureusMin;
                    if (Modifier.IsNotEmpty(itemOptions.AureusMax))
                        JQ.Filters.Sanctum.Filters.Aureus.Max = itemOptions.AureusMax;
                }

                JQ.Filters.Sanctum.Disabled = false;
            }
            else
            {
                JQ.Filters.Sanctum.Disabled = true;
            }

            JQ.Status = new(market);
            jsonData.Sort.Price = "asc";

            JQ.Filters.Trade.Disabled = DataManager.Config.Options.SearchBeforeDay == 0;

            if (DataManager.Config.Options.SearchBeforeDay != 0)
            {
                JQ.Filters.Trade.Filters.Indexed = new(BeforeDayToString(DataManager.Config.Options.SearchBeforeDay));
            }
            if (useSaleType)
            {
                JQ.Filters.Trade.Filters.SaleType = new("priced");
            }
            /*
            JQ.Filters.Trade.Filters.Price.Min = 99999;
            JQ.Filters.Trade.Filters.Price.Max = 99999;
            */
            if (itemOptions.PriceMin > 0 && Modifier.IsNotEmpty(itemOptions.PriceMin))
            {
                JQ.Filters.Trade.Filters.Price.Min = itemOptions.PriceMin;
            }

            JQ.Filters.Socket.Disabled = itemOptions.ChkSocket != true;

            if (Modifier.IsNotEmpty(itemOptions.LinkMin))
                JQ.Filters.Socket.Filters.Links.Min = itemOptions.LinkMin;
            if (Modifier.IsNotEmpty(itemOptions.LinkMax))
                JQ.Filters.Socket.Filters.Links.Max = itemOptions.LinkMax;

            if (Modifier.IsNotEmpty(itemOptions.SocketMin))
                JQ.Filters.Socket.Filters.Sockets.Min = itemOptions.SocketMin;
            if (Modifier.IsNotEmpty(itemOptions.SocketMax))
                JQ.Filters.Socket.Filters.Sockets.Max = itemOptions.SocketMax;

            if (itemOptions.SocketColors)
            {
                JQ.Filters.Socket.Filters.Sockets.Red = itemOptions.SocketRed;
                JQ.Filters.Socket.Filters.Sockets.Blue = itemOptions.SocketBlue;
                JQ.Filters.Socket.Filters.Sockets.Green = itemOptions.SocketGreen;
                JQ.Filters.Socket.Filters.Sockets.White = itemOptions.SocketWhite;
            }

            if (itemOptions.ChkQuality)
            {
                if (Modifier.IsNotEmpty(itemOptions.QualityMin))
                    JQ.Filters.Misc.Filters.Quality.Min = itemOptions.QualityMin;
                if (Modifier.IsNotEmpty(itemOptions.QualityMax))
                    JQ.Filters.Misc.Filters.Quality.Max = itemOptions.QualityMax;
            }

            if (Modifier.IsNotEmpty(itemOptions.FacetorExpMin))
                JQ.Filters.Misc.Filters.StoredExp.Min = itemOptions.FacetorExpMin;
            if (Modifier.IsNotEmpty(itemOptions.FacetorExpMax))
                JQ.Filters.Misc.Filters.StoredExp.Max = itemOptions.FacetorExpMax;

            if (!(!itemOptions.ChkLv || Inherit is Strings.Inherit.Gems || Inherit is Strings.Inherit.Maps || Inherit2 is Strings.Inherit.Area))
            {
                if (Inherit is not Strings.Inherit.Sanctum)
                {
                    if (Modifier.IsNotEmpty(itemOptions.LvMin))
                        JQ.Filters.Misc.Filters.Ilvl.Min = itemOptions.LvMin;
                    if (Modifier.IsNotEmpty(itemOptions.LvMax))
                        JQ.Filters.Misc.Filters.Ilvl.Max = itemOptions.LvMax;
                }
            }

            if (itemOptions.ChkLv && Inherit is Strings.Inherit.Gems)
            {
                if (Modifier.IsNotEmpty(itemOptions.LvMin ))
                    JQ.Filters.Misc.Filters.Gem_level.Min = itemOptions.LvMin;
                if (Modifier.IsNotEmpty(itemOptions.LvMax))
                    JQ.Filters.Misc.Filters.Gem_level.Max = itemOptions.LvMax;
            }

            if (Inherit is Strings.Inherit.Gems && itemOptions.AlternateQuality is not null)
            {
                JQ.Filters.Misc.Filters.Gem_alternate = new(itemOptions.AlternateQuality);
            }

            bool influenced = itemOptions.InfShaper || itemOptions.InfElder || itemOptions.InfCrusader
                || itemOptions.InfRedeemer || itemOptions.InfHunter || itemOptions.InfWarlord;

            /*
            JQ.Filters.Misc.Filters.Synthesis.Option = Strings.any;
            JQ.Filters.Misc.Filters.Split.Option = Strings.any;
            JQ.Filters.Misc.Filters.Mirrored.Option = Strings.any;
            */
            //JQ.Filters.Misc.Filters.Synthesis.Option = Inherit != Strings.Inherit.Maps && itemOptions.SynthesisBlight ? "true" : Strings.any;

            //JQ.Filters.Misc.Filters.Corrupted.Option = itemOptions.Corrupt == 1 ? "true" : (itemOptions.Corrupt == 2 ? "false" : "any");

            if (itemOptions.Corrupted is "true")
            {
                JQ.Filters.Misc.Filters.Corrupted = optTrue;
            }
            else if (itemOptions.Corrupted is "false")
            {
                JQ.Filters.Misc.Filters.Corrupted = optFalse;
            }

            JQ.Filters.Misc.Disabled = !(
                Modifier.IsNotEmpty(itemOptions.FacetorExpMin) || Modifier.IsNotEmpty(itemOptions.FacetorExpMax)
                || itemOptions.ChkQuality || Inherit is not Strings.Inherit.Maps && influenced || itemOptions.Corrupted is not Strings.any
                || Inherit is not Strings.Inherit.Maps && itemOptions.ChkLv || Inherit is not Strings.Inherit.Maps && (itemOptions.SynthesisBlight || itemOptions.BlightRavaged)
            );

            JQ.Filters.Map.Disabled = !(
                (Inherit == Strings.Inherit.Maps || Inherit2 is Strings.Inherit.Area || Inherit is Strings.Inherit.Sanctum) && (itemOptions.ChkLv || itemOptions.SynthesisBlight || itemOptions.BlightRavaged || itemOptions.Scourged || influenced)
            );

            if (itemOptions.ChkLv && Inherit is Strings.Inherit.Maps)
            {
                if (Modifier.IsNotEmpty(itemOptions.LvMin))
                    JQ.Filters.Map.Filters.Tier.Min = itemOptions.LvMin;
                if (Modifier.IsNotEmpty(itemOptions.LvMax))
                    JQ.Filters.Map.Filters.Tier.Max = itemOptions.LvMax;
            }

            if (itemOptions.ChkLv && (Inherit2 is Strings.Inherit.Area || Inherit is Strings.Inherit.Sanctum))
            {
                if (Modifier.IsNotEmpty(itemOptions.LvMin))
                    JQ.Filters.Map.Filters.Area.Min = itemOptions.LvMin;
                if (Modifier.IsNotEmpty(itemOptions.LvMax))
                    JQ.Filters.Map.Filters.Area.Max = itemOptions.LvMax;
            }

            if (Inherit is Strings.Inherit.Maps)
            {
                if (itemOptions.InfShaper)
                {
                    JQ.Filters.Map.Filters.Shaper = optTrue;
                }
                if (itemOptions.SynthesisBlight)
                {
                    JQ.Filters.Map.Filters.Blight = optTrue;
                }
                if (itemOptions.InfElder)
                {
                    JQ.Filters.Map.Filters.Elder = optTrue;
                }
                if (itemOptions.BlightRavaged)
                {
                    JQ.Filters.Map.Filters.BlightRavaged = optTrue;
                }

                if (itemOptions.ChkMapIiq)
                {
                    if (Modifier.IsNotEmpty(itemOptions.MapItemQuantityMin))
                        JQ.Filters.Map.Filters.Iiq.Min = itemOptions.MapItemQuantityMin;
                    if (Modifier.IsNotEmpty(itemOptions.MapItemQuantityMax))
                        JQ.Filters.Map.Filters.Iiq.Max = itemOptions.MapItemQuantityMax;
                }
                if (itemOptions.ChkMapIir)
                {
                    if (Modifier.IsNotEmpty(itemOptions.MapItemRarityMin))
                        JQ.Filters.Map.Filters.Iir.Min = itemOptions.MapItemRarityMin;
                    if (Modifier.IsNotEmpty(itemOptions.MapItemRarityMax))
                        JQ.Filters.Map.Filters.Iir.Max = itemOptions.MapItemRarityMax;
                }
                if (itemOptions.ChkMapPack)
                {
                    if (Modifier.IsNotEmpty(itemOptions.MapPackSizeMin))
                        JQ.Filters.Map.Filters.PackSize.Min = itemOptions.MapPackSizeMin;
                    if (Modifier.IsNotEmpty(itemOptions.MapPackSizeMax))
                        JQ.Filters.Map.Filters.PackSize.Max = itemOptions.MapPackSizeMax;
                }
            }

            if (itemOptions.Scourged)
            {
                JQ.Filters.Map.Filters.ScourgeTier.Min = 1;
                //JQ.Filters.Map.Filters.ScourgeTier.Max = 99999;
            }

            JQ.Filters.Ultimatum.Disabled = true;
            if (itemOptions.RewardType is not null && itemOptions.Reward is not null)
            {
                if (itemOptions.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards or Strings.Reward.MirrorRare or Strings.Reward.ExchangeUnique) // ultimatum
                {
                    JQ.Filters.Ultimatum.Disabled = false;
                    JQ.Filters.Ultimatum.Filters.Reward = new(itemOptions.RewardType);
                    if (itemOptions.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards)
                    {
                        JQ.Filters.Ultimatum.Filters.Input = new(itemOptions.Reward);
                    }
                    if (itemOptions.RewardType is Strings.Reward.ExchangeUnique)
                    {
                        JQ.Filters.Ultimatum.Filters.Output = new(itemOptions.Reward);
                    }
                }
                if (itemOptions.RewardType is Strings.Reward.FoilUnique) // valdo box
                {
                    JQ.Filters.Map.Filters.MapReward = new(itemOptions.Reward);
                }
            }

            bool error_filter = false;

            if (itemOptions.ItemFilters.Count > 0)
            {
                JQ.Stats = new Stats[1];
                JQ.Stats[0] = new()
                {
                    Type = "and",
                    Filters = new StatsFilters[itemOptions.ItemFilters.Count]
                };

                int idx = 0;

                for (int i = 0; i < itemOptions.ItemFilters.Count; i++)
                {
                    string input = itemOptions.ItemFilters[i].Text;
                    string id = itemOptions.ItemFilters[i].Id;
                    string type = itemOptions.ItemFilters[i].Id.Split('.')[0];
                    if (input.Trim().Length > 0)
                    {
                        string type_name = GetAffixType(type);

                        if (type_name.Length is 0)
                        {
                            continue; // will create a bad request as intended (to detect new type) and not crash the app 
                        }

                        FilterResultEntrie filter = null;

                        FilterResult filterResult = DataManager.Filter.Result.FirstOrDefault(x => x.Label == type_name);
                        type_name = type_name.ToLowerInvariant();
                        input = Regex.Escape(input).Replace("\\+\\#", "[+]?\\#");

                        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
                        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

                        // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                        string pseudo = rm.GetString("General014_Pseudo", cultureEn);

                        //bool isShako = DataManager.Words.FirstOrDefault(x => x.NameEn is "Forbidden Shako").Name == Modifier.CurrentItem.Name;
                        //if (type_name == pseudo && Inherit is Strings.Inherit.Weapons && Regex.IsMatch(id, @"^pseudo.pseudo_adds_[a-z]+_damage$"))
                        if (type_name == pseudo && Inherit is Strings.Inherit.Weapons && RegexUtil.AddsDamagePattern().IsMatch(id))
                        {
                            id += "_to_attacks";
                        }/*
                            else if (type_name != pseudo && (Inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours) && !isShako)
                            {
                                // Is the equipment only option (specific)
                                Regex rgx = new("^" + input + "$", RegexOptions.IgnoreCase);
                                filter = filterResult.Entries.FirstOrDefault(x => rgx.IsMatch(x.Text) && x.Type == type);
                            }*/

                        filter ??= filterResult.Entries.FirstOrDefault(x => x.ID == id && x.Type == type); // && x.Part == null

                        JQ.Stats[0].Filters[idx] = new();
                        JQ.Stats[0].Filters[idx].Value = new();
                        //JQ.Stats[0].Filters[idx].Value.Option = 99999;

                        if (filter is not null && filter.ID is not null && filter.ID.Trim().Length > 0)
                        {
                            JQ.Stats[0].Filters[idx].Disabled = itemOptions.ItemFilters[i].Disabled == true;

                            if (itemOptions.ItemFilters[i].Option != 0 && Modifier.IsNotEmpty(itemOptions.ItemFilters[i].Option))
                            {
                                JQ.Stats[0].Filters[idx].Value.Option = itemOptions.ItemFilters[i].Option;
                            }
                            else
                            {
                                if (Modifier.IsNotEmpty(itemOptions.ItemFilters[i].Min))
                                    JQ.Stats[0].Filters[idx].Value.Min = itemOptions.ItemFilters[i].Min;
                                if (Modifier.IsNotEmpty(itemOptions.ItemFilters[i].Max))
                                    JQ.Stats[0].Filters[idx].Value.Max = itemOptions.ItemFilters[i].Max;
                            }
                            JQ.Stats[0].Filters[idx++].Id = filter.ID;
                        }
                        else
                        {
                            error_filter = true;
                            itemOptions.ItemFilters[i].IsNull = true;

                            // Add anything on null to avoid errors
                            //JQ.Stats[0].Filters[idx].Disabled = true;
                            //JQ.Stats[0].Filters[idx++].Id = "error_id";
                        }
                    }
                }
            }

            // Set category here
            if (Strings.dicInherit.TryGetValue(Inherit, out string option))
            {
                if (itemOptions.ByType && Inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours)
                {
                    string[] lInherit = currentItem.Inherits;

                    if (lInherit.Length > 2)
                    {
                        string gearType = lInherit[Inherit == Strings.Inherit.Armours ? 1 : 2].ToLowerInvariant();

                        if (Inherit is Strings.Inherit.Weapons)
                        {
                            gearType = gearType.Replace("hand", string.Empty);
                            gearType = gearType.Remove(gearType.Length - 1);
                            if (gearType is "stave" && lInherit.Length == 4)
                            {
                                gearType = "staff";
                                /*
                                if (tmp[3] == "AbstractWarstaff")
                                    tmp2 = "warstaff";
                                else if (tmp[3] == "AbstractStaff")
                                    tmp2 = "staff";
                                */
                            }
                            else if (gearType is "onethrustingsword")
                            {
                                gearType = "onesword";
                            }
                        }
                        else if (Inherit == Strings.Inherit.Armours && gearType is "shields" or "helmets" or "bodyarmours")
                        {
                            gearType = gearType is "bodyarmours" ? "chest" : gearType.Remove(gearType.Length - 1);
                        }
                        option += "." + gearType;
                    }
                }

                if (!itemOptions.ByType && Inherit is Strings.Inherit.Currency)
                {
                    if (currentItem.TypeEn is "Forbidden Tome") // to redo
                    {
                        option = "sanctum.research";
                    }
                }

                JQ.Filters.Type.Filters.Category = new(option); // Item category
            }

            string rarityEn = GetEnglishRarity(itemOptions.Rarity);
            if (rarityEn.Length > 0)
            {
                rarityEn = rarityEn is "Any N-U" ? "nonunique"
                    : rarityEn is "Foil Unique" ? "uniquefoil"
                    : rarityEn.ToLowerInvariant();
                if (rarityEn is not Strings.any)
                {
                    JQ.Filters.Type.Filters.Rarity = new(rarityEn);
                }
            }

            if (itemOptions.ByType || currentItem.Name.Length == 0 ||
                itemOptions.Rarity != Resources.Resources.General006_Unique
                && itemOptions.Rarity != Resources.Resources.General110_FoilUnique)
            {
                if (!itemOptions.ByType && Inherit is not Strings.Inherit.Jewels
                    || Inherit is Strings.Inherit.NecropolisPack)
                {
                    bool isTransfiguredGem = Inherit is Strings.Inherit.Gems && Inherit2.Length > 0 && Inherit2.Contains("alt");
                    JQ.Type = !isTransfiguredGem ? currentItem.Type : new GemTransfigured() { Option = currentItem.Type, Discriminator = new(Inherit2) };
                }
            }
            else
            {
                JQ.Name = currentItem.Name;
                JQ.Type = currentItem.Type;
            }
            /*
            if (Inherit is Strings.Inherit.Gems && Inherit2.Length > 0 && Inherit2.Contains("alt"))
            {
                JQ.Disc = new(Inherit2);
            }*/
            if (itemOptions.ChaosDivOnly)
            {
                JQ.Filters.Trade.Disabled = false;
                JQ.Filters.Trade.Filters.Price.Option = new string("chaos_divine");
            }

            var json = Serialize<JsonData>(jsonData);

            if (error_filter)
            {
                int errorCount = 0;
                List<int> errors = new();
                for (int i = 0; i < itemOptions.ItemFilters.Count; i++)
                {
                    if (itemOptions.ItemFilters[i].IsNull)
                    {
                        errorCount++;
                        errors.Add(i + 1);
                    }
                }
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(string.Format("{0} Mod error(s) detected: \r\n\r\nMod lines : {1}\r\n\r\n", errorCount, errors.ToString()), "Mod Filter error", MessageStatus.Error);
            }
            return json;
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "JSON creation error", MessageStatus.Error);
        }
        return string.Empty;
    }

    private static string GetEnglishRarity(string rarityLang)
    {
        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        return rarityLang == Resources.Resources.General005_Any ? rm.GetString("General005_Any", cultureEn) :
            rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetString("General110_FoilUnique", cultureEn) :
            rarityLang == Resources.Resources.General006_Unique ? rm.GetString("General006_Unique", cultureEn) :
            rarityLang == Resources.Resources.General007_Rare ? rm.GetString("General007_Rare", cultureEn) :
            rarityLang == Resources.Resources.General008_Magic ? rm.GetString("General008_Magic", cultureEn) :
            rarityLang == Resources.Resources.General009_Normal ? rm.GetString("General009_Normal", cultureEn) :
            rarityLang == Resources.Resources.General010_AnyNU ? rm.GetString("General010_AnyNU", cultureEn) : string.Empty;
    }

    private static string GetAffixType(string inputType)
    {
        return inputType is "pseudo" ? Resources.Resources.General014_Pseudo :
            inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "fractured" ? Resources.Resources.General016_Fractured :
            inputType is "crafted" ? Resources.Resources.General012_Crafted :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "monster" ? Resources.Resources.General018_Monster :
            inputType is "veiled" ? Resources.Resources.General019_Veiled :
            inputType is "delve" ? Resources.Resources.General020_Delve :
            inputType is "ultimatum" ? Resources.Resources.General069_Ultimatum :
            inputType is "scourge" ? Resources.Resources.General099_Scourge :
            inputType is "crucible" ? Resources.Resources.General112_Crucible :
            inputType is "necropolis" ? Resources.Resources.General131_Necropolis :
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum : string.Empty;
    }
}
