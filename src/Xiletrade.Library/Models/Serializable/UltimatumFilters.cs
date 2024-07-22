using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class UltimatumFilters
{
    [DataMember(Name = "ultimatum_input", EmitDefaultValue = false)]
    public Options Input { get; set; }

    [DataMember(Name = "ultimatum_output", EmitDefaultValue = false)]
    public Options Output { get; set; }

    [DataMember(Name = "ultimatum_reward", EmitDefaultValue = false)]
    public Options Reward { get; set; }

    [DataMember(Name = "ultimatum_challenge", EmitDefaultValue = false)]
    public Options Challenge { get; set; }
}
