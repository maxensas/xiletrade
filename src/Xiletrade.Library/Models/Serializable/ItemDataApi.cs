using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ItemDataApi
{
    [JsonIgnore]
    public bool IsUnique => Rarity is not null && Rarity is "Unique";

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("baseType")]
    public string BaseType { get; set; }

    [JsonPropertyName("typeLine")]
    public string TypeLine { get; set; }

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; }

    [JsonPropertyName("enchantMods")]
    public string[] EnchantMods { get; set; }

    [JsonPropertyName("implicitMods")]
    public string[] ImplicitMods { get; set; }

    [JsonPropertyName("runeMods")]
    public string[] RuneMods { get; set; }

    [JsonPropertyName("explicitMods")]
    public string[] ExplicitMods { get; set; }

    [JsonPropertyName("desecratedMods")]
    public string[] DesecratedMods { get; set; }

    [JsonPropertyName("properties")]
    public ItemProperties[] Properties { get; set; }

    [JsonPropertyName("extended")]
    [JsonConverter(typeof(ItemExtendedOrEmptyArrayConverter))]
    public ItemExtended Extended { get; set; }

    public string GetModTooltip()
    {
        var enchants = EnchantMods is not null && EnchantMods.Length > 0;
        var implicits = ImplicitMods is not null && ImplicitMods.Length > 0;
        var explicits = ExplicitMods is not null && ExplicitMods.Length > 0;
        var desecrated = DesecratedMods is not null && DesecratedMods.Length > 0;
        if (Rarity is null || (!explicits && !implicits))
        {
            return null;
        }

        var lMods = new List<string>();
        if (Name is not null && BaseType is not null)
        {
            var header = (IsUnique ? Name : BaseType) + "\n";
            lMods.Add(header);

            bool dps = false;
            if (Extended?.Dps > 0)
            {
                lMods.Add(Resources.Resources.Main073_tbTotalDps + ": " + Extended.Dps);
                dps = true;
            }
            if (Extended?.Pdps > 0)
            {
                lMods.Add(Resources.Resources.Main074_tbPhysDps + ": " + Extended.Pdps);
                dps = true;
            }
            if (Extended?.Edps > 0)
            {
                lMods.Add(Resources.Resources.Main075_tbElemDps + ": " + Extended.Edps);
                dps = true;
            }
            if (dps)
            {
                lMods.Add("\r");
            }

            var properties = Properties is not null && Properties.Length > 1;
            if (properties)
            {
                var armourPiece = false;
                foreach (var prop in Properties)
                {
                    if (prop.Values is null || prop.Values.Count is not 1 || prop.Values[0].Item1 is null)
                    {
                        continue;
                    }
                    if (prop.Name.StartWith(Strings.ItemApi.Armour)
                        || prop.Name.StartWith(Strings.ItemApi.Evasion)
                        || prop.Name.StartWith(Strings.ItemApi.EnergyShield))
                    {
                        lMods.Add(prop.Name.ArrangeItemInfoDesc() + ": " + prop.Values[0].Item1);
                        armourPiece = true;
                    }
                }
                if (armourPiece)
                {
                    lMods.Add("\r");
                }
            }
        }
        if (enchants)
        {
            foreach (var mod in EnchantMods)
            {
                lMods.Add(Resources.Resources.General011_Enchant + ": " + mod.ArrangeItemInfoDesc());
            }
        }
        if (implicits)
        {
            foreach (var mod in ImplicitMods)
            {
                lMods.Add(Resources.Resources.General013_Implicit + ": " + mod.ArrangeItemInfoDesc());
            }
        }
        if (explicits)
        {
            if (implicits || enchants) lMods.Add("\r");
            foreach (var mod in ExplicitMods)
            {
                lMods.Add(mod.ArrangeItemInfoDesc());
            }
            if (desecrated)
            {
                foreach (var mod in DesecratedMods)
                {
                    lMods.Add(mod.ArrangeItemInfoDesc());
                }
            }
        }
        return lMods.Count > 0 ? string.Join("\n", lMods) : null;
    }

    /*
    [JsonPropertyName("realm")]
    [JsonIgnore]
    public string Realm { get; set; }

    [JsonPropertyName("verified")]
    [JsonIgnore]
    public bool Verified { get; set; }

    [JsonPropertyName("w")]
    [JsonIgnore]
    public int W { get; set; }

    [JsonPropertyName("h")]
    [JsonIgnore]
    public int H { get; set; }

    [JsonPropertyName("league")]
    [JsonIgnore]
    public string League { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore]
    public string Id { get; set; }

    [JsonPropertyName("ilvl")]
    [JsonIgnore]
    public int Ilvl { get; set; }

    [JsonPropertyName("identified")]
    [JsonIgnore]
    public bool Identified { get; set; }

    [JsonPropertyName("note")]
    [JsonIgnore]
    public string Note { get; set; }

    [JsonPropertyName("requirements")]
    [JsonIgnore]
    public object Requirements { get; set; }

    [JsonPropertyName("descrText")]
    [JsonIgnore]
    public string DescrText { get; set; }

    [JsonPropertyName("frameType")]
    [JsonIgnore]
    public int FrameType { get; set; }
    */
}
