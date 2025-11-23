using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract]
public sealed class BaseResultData
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "name_en")]
    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "id_class")]
    [JsonPropertyName("id_class")]
    public int ItemClassId { get; set; }

    [DataMember(Name = "inherits_from")]
    [JsonPropertyName("inherits_from")]
    public string InheritsFrom { get; set; } = null;
    /*
    [DataMember(Name = "BaseMonsterTypeIndex", EmitDefaultValue = false)]
    public string BaseMonsterTypeIndex { get; set; } = null;*/
}
