using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemDataApi
{
    [JsonIgnore]
    public bool IsUnique => Rarity is not null && Rarity is "Unique";

    [JsonPropertyName("isRelic")]
    public bool IsRelic { get; set; }

    [JsonPropertyName("identified")]
    public bool Identified { get; set; }

    [JsonPropertyName("corrupted")]
    public bool Corrupted { get; set; }

    [JsonPropertyName("doubleCorrupted")]
    public bool DoubleCorrupted { get; set; }

    [JsonPropertyName("desecrated")]
    public bool Desecrated { get; set; }

    [JsonPropertyName("foreseeing")]
    public bool Foreseeing { get; set; }

    [JsonPropertyName("fractured")]
    public bool Fractured { get; set; }

    [JsonPropertyName("sanctified")]
    public bool Sanctified { get; set; }

    [JsonPropertyName("mutated")]
    public bool Mutated { get; set; }

    [JsonPropertyName("veiled")]
    public bool Veiled { get; set; }

    [JsonPropertyName("split")]
    public bool Split { get; set; }

    [JsonPropertyName("searing")] // Searing Exarch Item
    public bool Searing { get; set; }

    [JsonPropertyName("tangled")] // Eater of Worlds Item
    public bool Tangled { get; set; }

    [JsonPropertyName("synthesised")]
    public bool Synthesised { get; set; }

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

    [JsonPropertyName("builtInSupport")]
    public string BuiltInSupport { get; set; }

    [JsonPropertyName("enchantMods")]
    public string[] EnchantMods { get; set; }

    [JsonPropertyName("implicitMods")]
    public string[] ImplicitMods { get; set; }

    [JsonPropertyName("runeMods")]
    public string[] RuneMods { get; set; }
  
    [JsonPropertyName("explicitMods")]
    public List<ModAffix> ExplicitMods { get; set; }

    [JsonPropertyName("veiledMods")]
    public string[] VeiledMods { get; set; }

    [JsonPropertyName("properties")]
    public ItemProperties[] Properties { get; set; }

    [JsonPropertyName("requirements")]
    public ItemProperties[] Requirements { get; set; }

    [JsonPropertyName("extended")]
    public ItemExtended Extended { get; set; }

    [JsonPropertyName("sockets")]
    public ItemSocket[] Sockets { get; set; }

    [JsonPropertyName("grantedSkills")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ItemGrantedSkill[] GrantedSkills { get; set; }

    [JsonPropertyName("w")]
    public int W { get; set; }

    [JsonPropertyName("h")]
    public int H { get; set; }
}
