using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Options of a custom search preset : true (yes), false (no), none : any.
/// </summary>
/// <remarks>twice_corrupted, desecrated, veiled and sanctified : poe 2 only, split : poe 1 only.</remarks>
public sealed class CustomSearchPresetOptions
{
    [JsonPropertyName("identified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Identified { get; set; } = null;

    [JsonPropertyName("corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Corrupted { get; set; } = null;

    [JsonPropertyName("twice_corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? TwiceCorrupted { get; set; } = null;

    [JsonPropertyName("mirrored")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Mirrored { get; set; } = null;

    [JsonPropertyName("fractured")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Fractured { get; set; } = null;

    [JsonPropertyName("split")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Split { get; set; } = null;

    [JsonPropertyName("crafted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Crafted { get; set; } = null;

    [JsonPropertyName("mutated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Mutated { get; set; } = null;

    [JsonPropertyName("desecrated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Desecrated { get; set; } = null;

    [JsonPropertyName("veiled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Veiled { get; set; } = null;

    [JsonPropertyName("sanctified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Sanctified { get; set; } = null;
}
