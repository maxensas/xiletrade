using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ConfigMods
{
    [DataMember(Name = "id")]
    public string ID { get; set; } = null;

    [DataMember(Name = "text")]
    public string Text { get; set; } = null;
}
