using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class SanctumFilters
{
    [JsonPropertyName("sanctum_resolve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Resolve { get; set; } = new();

    [JsonPropertyName("sanctum_max_resolve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MaxResolve { get; set; } = new();

    [JsonPropertyName("sanctum_inspiration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Inspiration { get; set; } = new();

    [JsonPropertyName("sanctum_gold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Aureus { get; set; } = new();
}
