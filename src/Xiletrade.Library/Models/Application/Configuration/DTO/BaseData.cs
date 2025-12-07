using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class BaseData
{
    [JsonPropertyName("result")]
    public BaseResult[] Result { get; set; } = null;
}
