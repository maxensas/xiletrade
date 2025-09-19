using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Application.Serialization.Converter;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedMagnitudes
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("min")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Min { get; set; }

    [JsonPropertyName("max")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Max { get; set; }
}
