using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public class QueryType
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Type { get; set; }
}
