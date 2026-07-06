using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class Identity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("stringId")]
    public string StringId { get; set; }

    [JsonPropertyName("dds")]
    public string Dds { get; set; } = null;
}
