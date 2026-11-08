using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Rule of StatRules.json (PoE 1) / StatRulesTwo.json (PoE 2) : id sent to the trade api for a filter entry,
/// according to the item (format : Data/StatRules.md, resolution : StatRuleExtensions).
/// The root of the file is a rule too (only 'mode' and 'rules').
/// </summary>
/// <remarks>Null and default values are not written : a file saved by the data update keeps the form of the repository one.</remarks>
public sealed class StatRule
{
    /// <summary>Free text for the readers of the file, not used.</summary>
    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Comment { get; set; }

    /// <summary>Conditions on the id of the entry (all must be true), none : any id.</summary>
    [JsonPropertyName("match")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatRuleMatch Match { get; set; }

    /// <summary>Conditions on the item (all must be true), none : any item.</summary>
    [JsonPropertyName("when")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] When { get; set; }

    /// <summary>Id replacing the current one.</summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Id { get; set; }

    /// <summary>Replaces the current id while keeping its first part, up to and including the '.' : "implicit." + suffixId.</summary>
    [JsonPropertyName("suffixId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string SuffixId { get; set; }

    /// <summary>The entry is not used for this item (next entry tried).</summary>
    [JsonPropertyName("skip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Skip { get; set; }

    /// <summary>How the child rules are applied : "all" (default) or "first".</summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Mode { get; set; }

    /// <summary>Child rules, applied once this rule applies.</summary>
    [JsonPropertyName("rules")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatRule[] Rules { get; set; }
}
