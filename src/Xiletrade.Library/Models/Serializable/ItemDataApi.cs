using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ItemDataApi
{
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

    [JsonPropertyName("explicitMods")]
    public string[] ExplicitMods { get; set; }

    public string GetModList()
    {
        var enchants = EnchantMods is not null && EnchantMods.Length > 0;
        var implicits = ImplicitMods is not null && ImplicitMods.Length > 0;
        var explicits = ExplicitMods is not null && ExplicitMods.Length > 0;
        if (Rarity is null || (!explicits && !implicits))
        {
            return null;
        }

        var lMods = new List<string>();
        if (Name is not null && BaseType is not null)
        {
            var header = (IsUnique ? Name : BaseType) + "\n";
            lMods.Add(header);
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

    [JsonPropertyName("properties")]
    [JsonIgnore]
    public object Properties { get; set; }

    [JsonPropertyName("requirements")]
    [JsonIgnore]
    public object Requirements { get; set; }

    [JsonPropertyName("descrText")]
    [JsonIgnore]
    public string DescrText { get; set; }

    [JsonPropertyName("frameType")]
    [JsonIgnore]
    public int FrameType { get; set; }

    [JsonPropertyName("extended")]
    [JsonIgnore]
    public object Extended { get; set; }
    */
}
