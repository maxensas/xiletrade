using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class OptionTxt(string option)
{
    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Option { get; set; } = option;
}
