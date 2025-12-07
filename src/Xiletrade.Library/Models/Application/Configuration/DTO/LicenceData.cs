using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class LicenceData
{
    [JsonPropertyName("licence")]
    public string Licence { get; set; } = null;
}
