using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class LeagueData
{
    [JsonPropertyName("result")]
    public LeagueResult[] Result { get; set; } = null;
}
