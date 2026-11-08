using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Conditions on the id of the entry, each list is satisfied when one of its values matches (ordinal comparisons).
/// </summary>
public sealed class StatRuleMatch
{
    /// <summary>The id is one of these ids.</summary>
    [JsonPropertyName("ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Ids { get; set; }

    /// <summary>The id ends with one of these values.</summary>
    [JsonPropertyName("suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Suffix { get; set; }

    /// <summary>The id contains one of these values.</summary>
    [JsonPropertyName("contains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Contains { get; set; }

    /// <summary>The id contains none of these values.</summary>
    [JsonPropertyName("notContains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] NotContains { get; set; }

    /// <summary>The stat part of the id (between the first and the second '.') is one of these values.</summary>
    [JsonPropertyName("stat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Stat { get; set; }

    /// <summary>Minimal length of the id.</summary>
    [JsonPropertyName("minLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MinLength { get; set; }
}
