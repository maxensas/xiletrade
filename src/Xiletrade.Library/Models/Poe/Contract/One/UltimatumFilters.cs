using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class UltimatumFilters
{
    [DataMember(Name = "ultimatum_input", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_input")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Input { get; set; }

    [DataMember(Name = "ultimatum_output", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Output { get; set; }

    [DataMember(Name = "ultimatum_reward", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Reward { get; set; }

    [DataMember(Name = "ultimatum_challenge", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_challenge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Challenge { get; set; }
}
