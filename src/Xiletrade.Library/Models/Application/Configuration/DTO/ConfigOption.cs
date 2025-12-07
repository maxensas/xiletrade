using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ConfigOption
{
    [JsonPropertyName("league")]
    public string League { get; set; } = null;

    [JsonPropertyName("language")]
    public int Language { get; set; } = 0;

    [JsonPropertyName("gateway")]
    public int Gateway { get; set; } = 0;

    /// <summary>
    /// Specify poe version used
    /// </summary>
    /// <remarks>
    /// poe1=0, poe2=1 
    /// </remarks>
    [JsonPropertyName("gameversion")]
    public int GameVersion { get; set; } = 0;

    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;

    [JsonPropertyName("search_fetch_detail")]
    public int SearchFetchDetail { get; set; } = 20;

    [JsonPropertyName("search_fetch_bulk")]
    public int SearchFetchBulk { get; set; } = 20;

    [JsonPropertyName("timeout_api_trade")]
    public int TimeoutTradeApi { get; set; } = 10; // GGG apis trade and fetch

    [JsonPropertyName("search_before_day")]
    public int SearchBeforeDay { get; set; } = 0;

    [JsonPropertyName("search_by_type")]
    public bool SearchByType { get; set; } = false;

    [JsonPropertyName("auto_check_non_uniques")]
    public bool AutoCheckNonUniques { get; set; } = false;

    [JsonPropertyName("auto_check_uniques")]
    public bool AutoCheckUniques { get; set; } = false;

    [JsonPropertyName("auto_check_crafted")]
    public bool AutoCheckCrafted { get; set; } = false;

    [JsonPropertyName("auto_check_corruptions")]
    public bool AutoCheckCorruptions { get; set; } = false;

    [JsonPropertyName("auto_check_implicits")]
    public bool AutoCheckImplicits { get; set; } = false;

    [JsonPropertyName("auto_check_enchants")]
    public bool AutoCheckEnchants { get; set; } = false;

    [JsonPropertyName("auto_select_pseudo")]
    public bool AutoSelectPseudo { get; set; } = false;

    [JsonPropertyName("auto_select_corrupt")]
    public bool AutoSelectCorrupt { get; set; } = false;

    [JsonPropertyName("auto_select_life")]
    public bool AutoSelectLife { get; set; } = false;

    [JsonPropertyName("auto_select_global_es")]
    public bool AutoSelectGlobalEs { get; set; } = false;

    [JsonPropertyName("auto_select_resists")]
    public bool AutoSelectRes { get; set; } = false;

    [JsonPropertyName("auto_select_areseva")]
    public bool AutoSelectArEsEva { get; set; } = false;

    [JsonPropertyName("auto_select_dps")]
    public bool AutoSelectDps { get; set; } = false;

    [JsonPropertyName("auto_select_mintiervalue")]
    public bool AutoSelectMinTierValue { get; set; } = false;

    [JsonPropertyName("auto_select_minpercentvalue")]
    public bool AutoSelectMinPercentValue { get; set; } = false;

    [JsonPropertyName("ctrl_wheel")]
    public bool CtrlWheel { get; set; } = false;

    [JsonPropertyName("check_updates")]
    public bool CheckUpdates { get; set; } = false;

    [JsonPropertyName("check_filters")]
    public bool CheckFilters { get; set; } = false;

    [JsonPropertyName("disable_startup_message")]
    public bool DisableStartupMessage { get; set; } = false;

    [JsonPropertyName("hide_same_occurs")]
    public bool HideSameOccurs { get; set; } = false;

    [JsonPropertyName("devmod")]
    public bool DevMode { get; set; } = false;

    [JsonPropertyName("autopaste")]
    public bool Autopaste { get; set; } = false;

    [JsonPropertyName("autoclose")]
    public bool Autoclose { get; set; } = false;

    [JsonPropertyName("opacity")]
    public double Opacity { get; set; } = 100;

    [JsonPropertyName("ninja_map_generation")]
    public string NinjaMapGeneration { get; set; } = null;

    [JsonPropertyName("market_default_async")]
    public bool AsyncMarketDefault { get; set; } = false;

    [JsonPropertyName("secret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Secret { get; set; }

    [JsonPropertyName("fast_inputs")]
    public bool FastInputs { get; set; } = false;
}
