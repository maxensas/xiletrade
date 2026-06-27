using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FlagAffix
{
    [JsonPropertyName("crafted")]
    public bool Crafted { get; set; }

    [JsonPropertyName("desecrated")]
    public bool Desecrated { get; set; }

    [JsonPropertyName("fractured")]
    public bool Fractured { get; set; }

    [JsonPropertyName("mutated")]
    public bool Mutated { get; set; }
}
