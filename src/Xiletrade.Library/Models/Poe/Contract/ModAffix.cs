using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ModAffix
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FlagAffix Flags { get; set; }

    [JsonPropertyName("mods")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExtendedAffix> Mods { get; set; }

    [JsonIgnore]
    public string Text { get; set; }
}
