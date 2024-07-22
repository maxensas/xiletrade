using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract(Name = "options")]
public sealed class ConfigOption
{
    [DataMember(Name = "league")]
    public string League { get; set; } = null;

    [DataMember(Name = "language")]
    public int Language { get; set; } = 0;
    /*
    [DataMember(Name = "server_timeout")]
    public int ServerTimeout = 0;

    [DataMember(Name = "server_redirect")]
    public bool ServerRedirect = false;
    */
    [DataMember(Name = "search_price_min")] // not used anymore
    public decimal SearchPriceMin { get; set; } = 0;

    [DataMember(Name = "search_fetch_detail")]
    public decimal SearchFetchDetail { get; set; } = 20;

    [DataMember(Name = "search_fetch_bulk")]
    public decimal SearchFetchBulk { get; set; } = 20;

    [DataMember(Name = "min_value_percent")]
    public double MinValuePercent { get; set; } = 100;

    [DataMember(Name = "max_value_percent")]
    public double MaxValuePercent { get; set; } = 100;

    [DataMember(Name = "unique_min_value_percent")]
    public double UniqueMinValuePercent { get; set; } = 100;

    [DataMember(Name = "unique_max_value_percent")]
    public double UniqueMaxValuePercent { get; set; } = 100;

    [DataMember(Name = "timeout_api_trade")]
    public int TimeoutTradeApi { get; set; } = 10; // GGG apis trade and fetch

    /*
    [DataMember(Name = "update_price_on_checked")]
    public bool UpdatePriceOnChecked = false;
    */
    [DataMember(Name = "search_before_day")]
    public int SearchBeforeDay { get; set; } = 0;

    [DataMember(Name = "search_by_type")]
    public bool SearchByType { get; set; } = false;

    /*
    [DataMember(Name = "auto_check_mods")]
    public bool AutoCheckMods = false;
    */
    [DataMember(Name = "auto_check_non_uniques")]
    public bool AutoCheckNonUniques { get; set; } = false;

    [DataMember(Name = "auto_check_uniques")]
    public bool AutoCheckUniques { get; set; } = false;

    [DataMember(Name = "auto_check_crafted")]
    public bool AutoCheckCrafted { get; set; } = false;

    [DataMember(Name = "auto_check_corruptions")]
    public bool AutoCheckCorruptions { get; set; } = false;

    [DataMember(Name = "auto_check_implicits")]
    public bool AutoCheckImplicits { get; set; } = false;

    [DataMember(Name = "auto_check_enchants")]
    public bool AutoCheckEnchants { get; set; } = false;

    [DataMember(Name = "auto_select_pseudo")]
    public bool AutoSelectPseudo { get; set; } = false;

    [DataMember(Name = "auto_select_corrupt")]
    public bool AutoSelectCorrupt { get; set; } = false;

    [DataMember(Name = "auto_select_life")]
    public bool AutoSelectLife { get; set; } = false;

    [DataMember(Name = "auto_select_global_es")]
    public bool AutoSelectGlobalEs { get; set; } = false;

    [DataMember(Name = "auto_select_resists")]
    public bool AutoSelectRes { get; set; } = false;

    [DataMember(Name = "auto_select_areseva")]
    public bool AutoSelectArEsEva { get; set; } = false;

    [DataMember(Name = "auto_select_dps")]
    public bool AutoSelectDps { get; set; } = false;

    [DataMember(Name = "auto_select_mintiervalue")]
    public bool AutoSelectMinTierValue { get; set; } = false;

    [DataMember(Name = "ctrl_wheel")]
    public bool CtrlWheel { get; set; } = false;

    [DataMember(Name = "check_updates")]
    public bool CheckUpdates { get; set; } = false;

    [DataMember(Name = "check_filters")]
    public bool CheckFilters { get; set; } = false;

    [DataMember(Name = "disable_startup_message")]
    public bool DisableStartupMessage { get; set; } = false;

    [DataMember(Name = "hide_same_occurs")]
    public bool HideSameOccurs { get; set; } = false;

    [DataMember(Name = "devmod")]
    public bool DevMode { get; set; } = false;

    [DataMember(Name = "autopaste")]
    public bool Autopaste { get; set; } = false;

    [DataMember(Name = "autopclose")]
    public bool Autoclose { get; set; } = false;

    [DataMember(Name = "opacity")]
    public double Opacity { get; set; } = 100;
}
