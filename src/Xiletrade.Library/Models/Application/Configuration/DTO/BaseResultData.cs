using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class BaseResultData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("id_class")]
    public int ItemClassId { get; set; }

    [JsonPropertyName("inherits_from")]
    public string InheritsFrom { get; set; } = null;
    /*
    [DataMember(Name = "BaseMonsterTypeIndex", EmitDefaultValue = false)]
    public string BaseMonsterTypeIndex { get; set; } = null;*/
}
