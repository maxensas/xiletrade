using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemDataApi
{
    [JsonIgnore]
    public bool IsUnique => Rarity is not null && Rarity is "Unique";

    [JsonPropertyName("isRelic")]
    public bool IsRelic { get; set; }

    [JsonPropertyName("corrupted")]
    public bool Corrupted { get; set; }

    [JsonPropertyName("ilvl")]
    public int Ilvl { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; }

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

    [JsonPropertyName("craftedMods")]
    public string[] CraftedMods { get; set; }

    [JsonPropertyName("fracturedMods")]
    public string[] FracturedMods { get; set; }

    [JsonPropertyName("mutatedMods")]
    public string[] MutatedMods { get; set; }

    [JsonPropertyName("properties")]
    public ItemProperties[] Properties { get; set; }

    [JsonPropertyName("extended")]
    public ItemExtended Extended { get; set; }

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
