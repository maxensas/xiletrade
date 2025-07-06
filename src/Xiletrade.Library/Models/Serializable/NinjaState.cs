using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class NinjaState
{
    [JsonPropertyName("economyLeagues")]
    public NinjaLeagues[] Leagues { get; set; } = null;
}
