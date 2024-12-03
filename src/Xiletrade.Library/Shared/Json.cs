using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Models.Serializable.SourceGeneration;
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
    /*
    // Old method will be removed.
    internal static string SerializeOld<T>(object obj) where T : class
    {
        return Utf8Json.JsonSerializer.ToJsonString((T)obj, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }

    // Old method will be removed.
    internal static T DeserializeOld<T>(string strData) where T : class
    {
        byte[] data = Encoding.UTF8.GetBytes(strData);
        return Utf8Json.JsonSerializer.Deserialize<T>(data, Utf8Json.Resolvers.StandardResolver.AllowPrivateExcludeNullSnakeCase);
    }
    */
    internal static string Serialize<T>(object obj) where T : class
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), SourceGenerationContext.ContextWithOptions)
            .Replace("\\u00A0", "\u00A0").Replace("\\u3000", "\u3000").Replace("\\u007F", "\u007f")
            .Replace("\\u0022", "\\\"").Replace("\\u0027", "\u0027"); //.Replace("\\u0026", "\u0026")
    }

    internal static T Deserialize<T>(string strData) where T : class
    {
        return System.Text.Json.JsonSerializer.Deserialize(strData, typeof(T), SourceGenerationContext.ContextWithOptions) as T;
    }

    internal static string GetSerialized(XiletradeItem xiletradeItem, ItemBaseName currentItem, bool useSaleType, string market)
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
            OptionTxt optTrue = new() { Option = "true" }, optFalse = new() { Option = "false" };

            string Inherit = currentItem.Inherits.Length > 0 ? currentItem.Inherits[0] : string.Empty;
            string Inherit2 = currentItem.Inherits.Length > 1 ? currentItem.Inherits[1] : string.Empty;

            JQ.Stats = []; // Array.Empty<Stats>();

            if (xiletradeItem.ChkArmour || xiletradeItem.ChkEnergy || xiletradeItem.ChkEvasion || xiletradeItem.ChkWard)
            {
                if (Modifier.IsNotEmpty(xiletradeItem.ArmourMin))
                    JQ.Filters.Armour.Filters.Armour.Min = xiletradeItem.ArmourMin;
                if (Modifier.IsNotEmpty(xiletradeItem.ArmourMax))
                    JQ.Filters.Armour.Filters.Armour.Max = xiletradeItem.ArmourMax;
                if (Modifier.IsNotEmpty(xiletradeItem.EnergyMin))
                    JQ.Filters.Armour.Filters.Energy.Min = xiletradeItem.EnergyMin;
                if (Modifier.IsNotEmpty(xiletradeItem.EnergyMax))
                    JQ.Filters.Armour.Filters.Energy.Max = xiletradeItem.EnergyMax;
                if (Modifier.IsNotEmpty(xiletradeItem.EvasionMin))
                    JQ.Filters.Armour.Filters.Evasion.Min = xiletradeItem.EvasionMin;
                if (Modifier.IsNotEmpty(xiletradeItem.EvasionMax))
                    JQ.Filters.Armour.Filters.Evasion.Max = xiletradeItem.EvasionMax;
                if (Modifier.IsNotEmpty(xiletradeItem.WardMin))
                    JQ.Filters.Armour.Filters.Ward.Min = xiletradeItem.WardMin;
                if (Modifier.IsNotEmpty(xiletradeItem.WardMax))
                    JQ.Filters.Armour.Filters.Ward.Max = xiletradeItem.WardMax;

                JQ.Filters.Armour.Disabled = false;
            }
            else
            {
                JQ.Filters.Armour.Disabled = true;
            }

            if (xiletradeItem.ChkDpsTotal || xiletradeItem.ChkDpsPhys || xiletradeItem.ChkDpsElem)
            {
                if (xiletradeItem.ChkDpsTotal)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsTotalMin))
                        JQ.Filters.Weapon.Filters.Damage.Min = xiletradeItem.DpsTotalMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsTotalMax))
                        JQ.Filters.Weapon.Filters.Damage.Max = xiletradeItem.DpsTotalMax;
                }
                if (xiletradeItem.ChkDpsPhys)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsPhysMin))
                        JQ.Filters.Weapon.Filters.Pdps.Min = xiletradeItem.DpsPhysMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsPhysMax))
                        JQ.Filters.Weapon.Filters.Pdps.Max = xiletradeItem.DpsPhysMax;
                }
                if (xiletradeItem.ChkDpsElem)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsElemMin))
                        JQ.Filters.Weapon.Filters.Edps.Min = xiletradeItem.DpsElemMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.DpsElemMax))
                        JQ.Filters.Weapon.Filters.Edps.Max = xiletradeItem.DpsElemMax;
                }

                JQ.Filters.Weapon.Disabled = false;
            }
            else
            {
                JQ.Filters.Weapon.Disabled = true;
            }

            if (xiletradeItem.ChkResolve || xiletradeItem.ChkMaxResolve || xiletradeItem.ChkInspiration || xiletradeItem.ChkAureus)
            {
                if (xiletradeItem.ChkResolve)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.ResolveMin))
                        JQ.Filters.Sanctum.Filters.Resolve.Min = xiletradeItem.ResolveMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.ResolveMax))
                        JQ.Filters.Sanctum.Filters.Resolve.Max = xiletradeItem.ResolveMax;
                }

                if (xiletradeItem.ChkMaxResolve)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.MaxResolveMin))
                        JQ.Filters.Sanctum.Filters.MaxResolve.Min = xiletradeItem.MaxResolveMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.MaxResolveMax))
                        JQ.Filters.Sanctum.Filters.MaxResolve.Max = xiletradeItem.MaxResolveMax;
                }

                if (xiletradeItem.ChkInspiration)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.InspirationMin))
                        JQ.Filters.Sanctum.Filters.Inspiration.Min = xiletradeItem.InspirationMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.InspirationMax))
                        JQ.Filters.Sanctum.Filters.Inspiration.Max = xiletradeItem.InspirationMax;
                }

                if (xiletradeItem.ChkAureus)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.AureusMin))
                        JQ.Filters.Sanctum.Filters.Aureus.Min = xiletradeItem.AureusMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.AureusMax))
                        JQ.Filters.Sanctum.Filters.Aureus.Max = xiletradeItem.AureusMax;
                }

                JQ.Filters.Sanctum.Disabled = false;
            }
            else
            {
                JQ.Filters.Sanctum.Disabled = true;
            }

            JQ.Status = new() { Option = market };
            jsonData.Sort.Price = "asc";

            JQ.Filters.Trade.Disabled = DataManager.Config.Options.SearchBeforeDay == 0;

            if (DataManager.Config.Options.SearchBeforeDay != 0)
            {
                JQ.Filters.Trade.Filters.Indexed = new() { Option = BeforeDayToString(DataManager.Config.Options.SearchBeforeDay) };
            }
            if (useSaleType)
            {
                JQ.Filters.Trade.Filters.SaleType = new() { Option = "priced" };
            }
            /*
            JQ.Filters.Trade.Filters.Price.Min = 99999;
            JQ.Filters.Trade.Filters.Price.Max = 99999;
            */
            if (xiletradeItem.PriceMin > 0 && Modifier.IsNotEmpty(xiletradeItem.PriceMin))
            {
                JQ.Filters.Trade.Filters.Price.Min = xiletradeItem.PriceMin;
            }

            JQ.Filters.Socket.Disabled = xiletradeItem.ChkSocket != true;

            if (Modifier.IsNotEmpty(xiletradeItem.LinkMin))
                JQ.Filters.Socket.Filters.Links.Min = xiletradeItem.LinkMin;
            if (Modifier.IsNotEmpty(xiletradeItem.LinkMax))
                JQ.Filters.Socket.Filters.Links.Max = xiletradeItem.LinkMax;

            if (Modifier.IsNotEmpty(xiletradeItem.SocketMin))
                JQ.Filters.Socket.Filters.Sockets.Min = xiletradeItem.SocketMin;
            if (Modifier.IsNotEmpty(xiletradeItem.SocketMax))
                JQ.Filters.Socket.Filters.Sockets.Max = xiletradeItem.SocketMax;

            if (xiletradeItem.SocketColors)
            {
                JQ.Filters.Socket.Filters.Sockets.Red = xiletradeItem.SocketRed;
                JQ.Filters.Socket.Filters.Sockets.Blue = xiletradeItem.SocketBlue;
                JQ.Filters.Socket.Filters.Sockets.Green = xiletradeItem.SocketGreen;
                JQ.Filters.Socket.Filters.Sockets.White = xiletradeItem.SocketWhite;
            }

            if (xiletradeItem.ChkQuality)
            {
                if (Modifier.IsNotEmpty(xiletradeItem.QualityMin))
                    JQ.Filters.Misc.Filters.Quality.Min = xiletradeItem.QualityMin;
                if (Modifier.IsNotEmpty(xiletradeItem.QualityMax))
                    JQ.Filters.Misc.Filters.Quality.Max = xiletradeItem.QualityMax;
            }

            if (Modifier.IsNotEmpty(xiletradeItem.FacetorExpMin))
                JQ.Filters.Misc.Filters.StoredExp.Min = xiletradeItem.FacetorExpMin;
            if (Modifier.IsNotEmpty(xiletradeItem.FacetorExpMax))
                JQ.Filters.Misc.Filters.StoredExp.Max = xiletradeItem.FacetorExpMax;

            if (!(!xiletradeItem.ChkLv || Inherit is Strings.Inherit.Gems || Inherit is Strings.Inherit.Maps || Inherit2 is Strings.Inherit.Area))
            {
                if (Inherit is not Strings.Inherit.Sanctum)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.LvMin))
                        JQ.Filters.Misc.Filters.Ilvl.Min = xiletradeItem.LvMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.LvMax))
                        JQ.Filters.Misc.Filters.Ilvl.Max = xiletradeItem.LvMax;
                }
            }

            if (xiletradeItem.ChkLv && Inherit is Strings.Inherit.Gems)
            {
                if (Modifier.IsNotEmpty(xiletradeItem.LvMin ))
                    JQ.Filters.Misc.Filters.Gem_level.Min = xiletradeItem.LvMin;
                if (Modifier.IsNotEmpty(xiletradeItem.LvMax))
                    JQ.Filters.Misc.Filters.Gem_level.Max = xiletradeItem.LvMax;
            }

            if (Inherit is Strings.Inherit.Gems && xiletradeItem.AlternateQuality is not null)
            {
                JQ.Filters.Misc.Filters.Gem_alternate = new() { Option = xiletradeItem.AlternateQuality }; 
            }

            bool influenced = xiletradeItem.InfShaper || xiletradeItem.InfElder || xiletradeItem.InfCrusader
                || xiletradeItem.InfRedeemer || xiletradeItem.InfHunter || xiletradeItem.InfWarlord;

            /*
            JQ.Filters.Misc.Filters.Synthesis.Option = Strings.any;
            JQ.Filters.Misc.Filters.Split.Option = Strings.any;
            JQ.Filters.Misc.Filters.Mirrored.Option = Strings.any;
            */
            //JQ.Filters.Misc.Filters.Synthesis.Option = Inherit != Strings.Inherit.Maps && itemOptions.SynthesisBlight ? "true" : Strings.any;

            //JQ.Filters.Misc.Filters.Corrupted.Option = itemOptions.Corrupt == 1 ? "true" : (itemOptions.Corrupt == 2 ? "false" : "any");

            if (xiletradeItem.Corrupted is "true")
            {
                JQ.Filters.Misc.Filters.Corrupted = optTrue;
            }
            else if (xiletradeItem.Corrupted is "false")
            {
                JQ.Filters.Misc.Filters.Corrupted = optFalse;
            }

            JQ.Filters.Misc.Disabled = !(
                Modifier.IsNotEmpty(xiletradeItem.FacetorExpMin) || Modifier.IsNotEmpty(xiletradeItem.FacetorExpMax)
                || xiletradeItem.ChkQuality || Inherit is not Strings.Inherit.Maps && influenced || xiletradeItem.Corrupted is not Strings.any
                || Inherit is not Strings.Inherit.Maps && xiletradeItem.ChkLv || Inherit is not Strings.Inherit.Maps && (xiletradeItem.SynthesisBlight || xiletradeItem.BlightRavaged)
            );

            JQ.Filters.Map.Disabled = !(
                (Inherit == Strings.Inherit.Maps || Inherit2 is Strings.Inherit.Area || Inherit is Strings.Inherit.Sanctum) && (xiletradeItem.ChkLv || xiletradeItem.SynthesisBlight || xiletradeItem.BlightRavaged || xiletradeItem.Scourged || influenced)
            );

            if (xiletradeItem.ChkLv && Inherit is Strings.Inherit.Maps)
            {
                if (Modifier.IsNotEmpty(xiletradeItem.LvMin))
                    JQ.Filters.Map.Filters.Tier.Min = xiletradeItem.LvMin;
                if (Modifier.IsNotEmpty(xiletradeItem.LvMax))
                    JQ.Filters.Map.Filters.Tier.Max = xiletradeItem.LvMax;
            }

            if (xiletradeItem.ChkLv && (Inherit2 is Strings.Inherit.Area || Inherit is Strings.Inherit.Sanctum))
            {
                if (Modifier.IsNotEmpty(xiletradeItem.LvMin))
                    JQ.Filters.Map.Filters.Area.Min = xiletradeItem.LvMin;
                if (Modifier.IsNotEmpty(xiletradeItem.LvMax))
                    JQ.Filters.Map.Filters.Area.Max = xiletradeItem.LvMax;
            }

            if (Inherit is Strings.Inherit.Maps)
            {
                if (xiletradeItem.InfShaper)
                {
                    JQ.Filters.Map.Filters.Shaper = optTrue;
                }
                if (xiletradeItem.SynthesisBlight)
                {
                    JQ.Filters.Map.Filters.Blight = optTrue;
                }
                if (xiletradeItem.InfElder)
                {
                    JQ.Filters.Map.Filters.Elder = optTrue;
                }
                if (xiletradeItem.BlightRavaged)
                {
                    JQ.Filters.Map.Filters.BlightRavaged = optTrue;
                }

                if (xiletradeItem.ChkMapIiq)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.MapItemQuantityMin))
                        JQ.Filters.Map.Filters.Iiq.Min = xiletradeItem.MapItemQuantityMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.MapItemQuantityMax))
                        JQ.Filters.Map.Filters.Iiq.Max = xiletradeItem.MapItemQuantityMax;
                }
                if (xiletradeItem.ChkMapIir)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.MapItemRarityMin))
                        JQ.Filters.Map.Filters.Iir.Min = xiletradeItem.MapItemRarityMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.MapItemRarityMax))
                        JQ.Filters.Map.Filters.Iir.Max = xiletradeItem.MapItemRarityMax;
                }
                if (xiletradeItem.ChkMapPack)
                {
                    if (Modifier.IsNotEmpty(xiletradeItem.MapPackSizeMin))
                        JQ.Filters.Map.Filters.PackSize.Min = xiletradeItem.MapPackSizeMin;
                    if (Modifier.IsNotEmpty(xiletradeItem.MapPackSizeMax))
                        JQ.Filters.Map.Filters.PackSize.Max = xiletradeItem.MapPackSizeMax;
                }
            }

            if (xiletradeItem.Scourged)
            {
                JQ.Filters.Map.Filters.ScourgeTier.Min = 1;
                //JQ.Filters.Map.Filters.ScourgeTier.Max = 99999;
            }

            JQ.Filters.Ultimatum.Disabled = true;
            if (xiletradeItem.RewardType is not null && xiletradeItem.Reward is not null)
            {
                if (xiletradeItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards or Strings.Reward.MirrorRare or Strings.Reward.ExchangeUnique) // ultimatum
                {
                    JQ.Filters.Ultimatum.Disabled = false;
                    JQ.Filters.Ultimatum.Filters.Reward = new() { Option = xiletradeItem.RewardType };
                    if (xiletradeItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards)
                    {
                        JQ.Filters.Ultimatum.Filters.Input = new() { Option = xiletradeItem.Reward };
                    }
                    if (xiletradeItem.RewardType is Strings.Reward.ExchangeUnique)
                    {
                        JQ.Filters.Ultimatum.Filters.Output = new() { Option = xiletradeItem.Reward };
                    }
                }
                if (xiletradeItem.RewardType is Strings.Reward.FoilUnique) // valdo box
                {
                    JQ.Filters.Map.Filters.MapReward = new() { Option = xiletradeItem.Reward };
                }
            }

            bool error_filter = false;

            if (xiletradeItem.ItemFilters.Count > 0)
            {
                JQ.Stats = new Stats[1];
                JQ.Stats[0] = new()
                {
                    Type = "and",
                    Filters = new StatsFilters[xiletradeItem.ItemFilters.Count]
                };

                int idx = 0;

                for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
                {
                    string input = xiletradeItem.ItemFilters[i].Text;
                    string id = xiletradeItem.ItemFilters[i].Id;
                    string type = xiletradeItem.ItemFilters[i].Id.Split('.')[0];
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
                            JQ.Stats[0].Filters[idx].Disabled = xiletradeItem.ItemFilters[i].Disabled == true;

                            if (xiletradeItem.ItemFilters[i].Option != 0 && Modifier.IsNotEmpty(xiletradeItem.ItemFilters[i].Option))
                            {
                                JQ.Stats[0].Filters[idx].Value.Option = xiletradeItem.ItemFilters[i].Option.ToString();
                            }
                            else
                            {
                                if (Modifier.IsNotEmpty(xiletradeItem.ItemFilters[i].Min))
                                    JQ.Stats[0].Filters[idx].Value.Min = xiletradeItem.ItemFilters[i].Min;
                                if (Modifier.IsNotEmpty(xiletradeItem.ItemFilters[i].Max))
                                    JQ.Stats[0].Filters[idx].Value.Max = xiletradeItem.ItemFilters[i].Max;
                            }
                            JQ.Stats[0].Filters[idx++].Id = filter.ID;
                        }
                        else
                        {
                            error_filter = true;
                            xiletradeItem.ItemFilters[i].IsNull = true;

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
                if (xiletradeItem.ByType && Inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours)
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

                if (!xiletradeItem.ByType && Inherit is Strings.Inherit.Currency)
                {
                    if (currentItem.TypeEn is "Forbidden Tome") // to redo
                    {
                        option = "sanctum.research";
                    }
                }

                JQ.Filters.Type.Filters.Category = new() { Option = option }; // Item category
            }

            string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
            if (rarityEn.Length > 0)
            {
                rarityEn = rarityEn is "Any N-U" ? "nonunique"
                    : rarityEn is "Foil Unique" ? "uniquefoil"
                    : rarityEn.ToLowerInvariant();
                if (rarityEn is not Strings.any)
                {
                    JQ.Filters.Type.Filters.Rarity = new() { Option = rarityEn };
                }
            }

            if (xiletradeItem.ByType || currentItem.Name.Length == 0 ||
                xiletradeItem.Rarity != Resources.Resources.General006_Unique
                && xiletradeItem.Rarity != Resources.Resources.General110_FoilUnique)
            {
                if (!xiletradeItem.ByType && Inherit is not Strings.Inherit.Jewels
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
            if (xiletradeItem.ChaosDivOnly)
            {
                JQ.Filters.Trade.Disabled = false;
                JQ.Filters.Trade.Filters.Price.Option = new string("chaos_divine");
            }

            var json = Serialize<JsonData>(jsonData);

            if (error_filter)
            {
                int errorCount = 0;
                List<int> errors = new();
                for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
                {
                    if (xiletradeItem.ItemFilters[i].IsNull)
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
