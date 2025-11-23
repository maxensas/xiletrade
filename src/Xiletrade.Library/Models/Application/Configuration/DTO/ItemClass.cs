using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ItemClass
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("id_origin")]
    public string IdOrigin { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;
}
