using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ItemDataApi
{
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("typeLine")]
    public string TypeLine { get; set; }

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; }

    [JsonPropertyName("explicitMods")]
    public string[] ExplicitMods { get; set; }

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

    [JsonPropertyName("baseType")]
    [JsonIgnore]
    public string BaseType { get; set; }

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
