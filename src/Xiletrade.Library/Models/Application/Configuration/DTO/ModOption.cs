using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ModOption
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("stat")]
    public string Stat { get; set; } = string.Empty;

    [JsonPropertyName("replace")]
    public string Replace { get; set; } = string.Empty;

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("old")]
    public string Old { get; set; } = string.Empty;

    [JsonPropertyName("new")]
    public string New { get; set; } = string.Empty;
}
