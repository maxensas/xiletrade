using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemSocket
{
    [JsonPropertyName("group")]
    public int Group { get; set; }

    #region POE1
    [JsonPropertyName("attr")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Attribute { get; set; }

    [JsonPropertyName("sColour")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Color { get; set; }
    #endregion

    #region POE2
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; }
    #endregion
}
