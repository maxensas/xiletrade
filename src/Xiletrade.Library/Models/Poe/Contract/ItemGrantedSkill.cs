using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemGrantedSkill
{
    [JsonPropertyName("displayMode")]
    public int DisplayMode { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("values")]
    public List<(string, int)> Values { get; set; }
}
