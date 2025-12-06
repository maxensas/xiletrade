using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaItemContract
{
    [JsonPropertyName("lines")]
    public NinjaItemLines[] Lines { get; set; } = null;
}
