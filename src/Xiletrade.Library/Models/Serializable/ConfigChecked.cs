using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract(Name = "checked")]
public sealed class ConfigChecked
{
    [DataMember(Name = "id")]
    public string ID { get; set; } = null;

    [DataMember(Name = "text")]
    public string Text { get; set; } = null;

    [DataMember(Name = "mod_type")]
    public string ModType { get; set; } = null;
}
