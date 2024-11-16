using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract(Name = "options")]
public sealed class ConfigOption
{
    [DataMember(Name = "league")]
    [JsonPropertyName("league")]
    public string League { get; set; } = null;

    [DataMember(Name = "language")]
    [JsonPropertyName("language")]
    public int Language { get; set; } = 0;

    [DataMember(Name = "search_fetch_detail")]
    [JsonPropertyName("search_fetch_detail")]
    public decimal SearchFetchDetail { get; set; } = 20;

    [DataMember(Name = "search_fetch_bulk")]
    [JsonPropertyName("search_fetch_bulk")]
    public decimal SearchFetchBulk { get; set; } = 20;

    [DataMember(Name = "timeout_api_trade")]
    [JsonPropertyName("timeout_api_trade")]
    public int TimeoutTradeApi { get; set; } = 10; // GGG apis trade and fetch

    [DataMember(Name = "search_before_day")]
    [JsonPropertyName("search_before_day")]
    public int SearchBeforeDay { get; set; } = 0;

    [DataMember(Name = "search_by_type")]
    [JsonPropertyName("search_by_type")]
    public bool SearchByType { get; set; } = false;

    [DataMember(Name = "auto_check_non_uniques")]
    [JsonPropertyName("auto_check_non_uniques")]
    public bool AutoCheckNonUniques { get; set; } = false;

    [DataMember(Name = "auto_check_uniques")]
    [JsonPropertyName("auto_check_uniques")]
    public bool AutoCheckUniques { get; set; } = false;

    [DataMember(Name = "auto_check_crafted")]
    [JsonPropertyName("auto_check_crafted")]
    public bool AutoCheckCrafted { get; set; } = false;

    [DataMember(Name = "auto_check_corruptions")]
    [JsonPropertyName("auto_check_corruptions")]
    public bool AutoCheckCorruptions { get; set; } = false;

    [DataMember(Name = "auto_check_implicits")]
    [JsonPropertyName("auto_check_implicits")]
    public bool AutoCheckImplicits { get; set; } = false;

    [DataMember(Name = "auto_check_enchants")]
    [JsonPropertyName("auto_check_enchants")]
    public bool AutoCheckEnchants { get; set; } = false;

    [DataMember(Name = "auto_select_pseudo")]
    [JsonPropertyName("auto_select_pseudo")]
    public bool AutoSelectPseudo { get; set; } = false;

    [DataMember(Name = "auto_select_corrupt")]
    [JsonPropertyName("auto_select_corrupt")]
    public bool AutoSelectCorrupt { get; set; } = false;

    [DataMember(Name = "auto_select_life")]
    [JsonPropertyName("auto_select_life")]
    public bool AutoSelectLife { get; set; } = false;

    [DataMember(Name = "auto_select_global_es")]
    [JsonPropertyName("auto_select_global_es")]
    public bool AutoSelectGlobalEs { get; set; } = false;

    [DataMember(Name = "auto_select_resists")]
    [JsonPropertyName("auto_select_resists")]
    public bool AutoSelectRes { get; set; } = false;

    [DataMember(Name = "auto_select_areseva")]
    [JsonPropertyName("auto_select_areseva")]
    public bool AutoSelectArEsEva { get; set; } = false;

    [DataMember(Name = "auto_select_dps")]
    [JsonPropertyName("auto_select_dps")]
    public bool AutoSelectDps { get; set; } = false;

    [DataMember(Name = "auto_select_mintiervalue")]
    [JsonPropertyName("auto_select_mintiervalue")]
    public bool AutoSelectMinTierValue { get; set; } = false;

    [DataMember(Name = "ctrl_wheel")]
    [JsonPropertyName("ctrl_wheel")]
    public bool CtrlWheel { get; set; } = false;

    [DataMember(Name = "check_updates")]
    [JsonPropertyName("check_updates")]
    public bool CheckUpdates { get; set; } = false;

    [DataMember(Name = "check_filters")]
    [JsonPropertyName("check_filters")]
    public bool CheckFilters { get; set; } = false;

    [DataMember(Name = "disable_startup_message")]
    [JsonPropertyName("disable_startup_message")]
    public bool DisableStartupMessage { get; set; } = false;

    [DataMember(Name = "hide_same_occurs")]
    [JsonPropertyName("hide_same_occurs")]
    public bool HideSameOccurs { get; set; } = false;

    [DataMember(Name = "devmod")]
    [JsonPropertyName("devmod")]
    public bool DevMode { get; set; } = false;

    [DataMember(Name = "autopaste")]
    [JsonPropertyName("autopaste")]
    public bool Autopaste { get; set; } = false;

    [DataMember(Name = "autoclose")]
    [JsonPropertyName("autoclose")]
    public bool Autoclose { get; set; } = false;

    [DataMember(Name = "opacity")]
    [JsonPropertyName("opacity")]
    public double Opacity { get; set; } = 100;

    [DataMember(Name = "ninja_map_generation")]
    [JsonPropertyName("ninja_map_generation")]
    public string NinjaMapGeneration { get; set; } = null;
}
