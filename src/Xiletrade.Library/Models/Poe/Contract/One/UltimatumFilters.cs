using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class UltimatumFilters
{
    [JsonPropertyName("ultimatum_input")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Input { get; set; }

    [JsonPropertyName("ultimatum_output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Output { get; set; }

    [JsonPropertyName("ultimatum_reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Reward { get; set; }

    [JsonPropertyName("ultimatum_challenge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Challenge { get; set; }
}
