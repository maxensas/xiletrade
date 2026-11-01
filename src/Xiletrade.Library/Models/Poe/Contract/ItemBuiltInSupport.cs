using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

/// <summary>
/// "builtInSupport" of the trade api : usually a string (description only),
/// an object with the domain and hash for imbued skill gems.
/// </summary>
public sealed class ItemBuiltInSupport
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}
