using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class SearchPresetData
{
    [JsonPropertyName("unidentified_unique")]
    public UniqueUnidentified[] UnidUnique { get; set; } = null;
}
