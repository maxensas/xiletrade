using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BaseResultData
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "nameEn")]
    [JsonPropertyName("nameEn")]
    public string NameEn { get; set; } = null;

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "inheritsFrom")]
    [JsonPropertyName("inheritsFrom")]
    public string InheritsFrom { get; set; } = null;
    /*
    [DataMember(Name = "BaseMonsterTypeIndex", EmitDefaultValue = false)]
    public string BaseMonsterTypeIndex { get; set; } = null;*/
}
