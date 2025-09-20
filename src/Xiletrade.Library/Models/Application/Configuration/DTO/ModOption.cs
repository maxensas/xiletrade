using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract()]
public sealed class ModOption
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [DataMember(Name = "stat")]
    [JsonPropertyName("stat")]
    public string Stat { get; set; } = string.Empty;

    [DataMember(Name = "replace")]
    [JsonPropertyName("replace")]
    public string Replace { get; set; } = string.Empty;

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [DataMember(Name = "old")]
    [JsonPropertyName("old")]
    public string Old { get; set; } = string.Empty;

    [DataMember(Name = "new")]
    [JsonPropertyName("new")]
    public string New { get; set; } = string.Empty;
}
