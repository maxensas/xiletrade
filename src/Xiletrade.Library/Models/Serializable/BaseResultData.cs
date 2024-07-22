using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BaseResultData
{
    [DataMember(Name = "Id")]
    public string ID { get; set; } = null;

    [DataMember(Name = "NameEn")]
    public string NameEn { get; set; } = null;

    [DataMember(Name = "Name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "InheritsFrom")]
    public string InheritsFrom { get; set; } = null;

    [DataMember(Name = "BaseMonsterTypeIndex", EmitDefaultValue = false)]
    public string MonsterTypeIndex { get; set; } = null;
}
