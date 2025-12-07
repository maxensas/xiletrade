using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class SocketFilters
{
    [JsonPropertyName("sockets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Sockets Sockets { get; set; } = new(); //public MinMax Sockets { get; set; } = new();

    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Links { get; set; } = new();
}
