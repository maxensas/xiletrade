using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaState
{
    [JsonPropertyName("economyLeagues")]
    public NinjaLeagues[] Leagues { get; set; } = null;
}
