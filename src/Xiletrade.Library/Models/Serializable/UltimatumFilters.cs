using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class UltimatumFilters
{
    [DataMember(Name = "ultimatum_input", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_input")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Input { get; set; }

    [DataMember(Name = "ultimatum_output", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Output { get; set; }

    [DataMember(Name = "ultimatum_reward", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Reward { get; set; }

    [DataMember(Name = "ultimatum_challenge", EmitDefaultValue = false)]
    [JsonPropertyName("ultimatum_challenge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Challenge { get; set; }
}
