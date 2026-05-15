using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class ItemOption
{
    private readonly Dictionary<string, string> _options = new(StringComparer.Ordinal)
    {
        [Resources.Resources.General035_Quality] = string.Empty,
        [Resources.Resources.General031_Lv] = string.Empty,
        [Resources.Resources.General032_ItemLv] = string.Empty,
        [Resources.Resources.General034_MaTier] = string.Empty,
        [Resources.Resources.General143_WaystoneTier] = string.Empty,
        [Resources.Resources.General067_AreaLevel] = string.Empty,
        [Resources.Resources.General198_AreaLevelBis] = string.Empty,
        [Resources.Resources.General036_Socket] = string.Empty,
        [Resources.Resources.General055_Armour] = string.Empty,
        [Resources.Resources.General056_Energy] = string.Empty,
        [Resources.Resources.General057_Evasion] = string.Empty,
        [Resources.Resources.General095_Ward] = string.Empty,
        [Resources.Resources.General058_PhysicalDamage] = string.Empty,
        [Resources.Resources.General059_ElementalDamage] = string.Empty,
        [Resources.Resources.General060_ChaosDamage] = string.Empty,
        [Resources.Resources.General148_ColdDamage] = string.Empty,
        [Resources.Resources.General149_FireDamage] = string.Empty,
        [Resources.Resources.General146_LightningDamage] = string.Empty,
        [Resources.Resources.General061_AttacksPerSecond] = string.Empty,
        [Resources.Resources.General147_CriticalHitChance] = string.Empty,
        [Resources.Resources.General196_StoredExperience] = string.Empty,
        [Resources.Resources.General070_ReqSacrifice] = string.Empty,
        [Resources.Resources.General071_Reward] = string.Empty,
        [Resources.Resources.General114_SanctumResolve] = string.Empty,
        [Resources.Resources.General115_SanctumInspiration] = string.Empty,
        [Resources.Resources.General116_SanctumAureus] = string.Empty,
        [Resources.Resources.General117_SanctumMinorBoons] = string.Empty,
        [Resources.Resources.General118_SanctumMajorBoons] = string.Empty,
        [Resources.Resources.General119_SanctumMinorAfflictions] = string.Empty,
        [Resources.Resources.General120_SanctumMajorAfflictions] = string.Empty,
        [Resources.Resources.General123_SanctumPacts] = string.Empty,
        [Resources.Resources.General121_RewardsFloorCompletion] = string.Empty,
        [Resources.Resources.General122_RewardsSanctumCompletion] = string.Empty,
        [Resources.Resources.General128_Monster] = string.Empty,
        [Resources.Resources.General129_CorpseLevel] = string.Empty,
        [Resources.Resources.General130_MonsterCategory] = string.Empty,
        [Resources.Resources.General136_ItemQuantity] = string.Empty,
        [Resources.Resources.General137_ItemRarity] = string.Empty,
        [Resources.Resources.General138_MonsterPackSize] = string.Empty,
        [Resources.Resources.General139_MoreCurrency] = string.Empty,
        [Resources.Resources.General140_MoreScarabs] = string.Empty,
        [Resources.Resources.General141_MoreMaps] = string.Empty,
        [Resources.Resources.General142_MoreDivinationCards] = string.Empty,
        [Resources.Resources.General162_RareMonsters] = string.Empty,
        [Resources.Resources.General161_MagicMonsters] = string.Empty,
        [Resources.Resources.General155_Requires] = string.Empty,
        [Resources.Resources.General156_MemoryStrands] = string.Empty,
    };

    private string Get(string key) => _options.TryGetValue(key, out var value) ? value : string.Empty;
    private string[] SplitComma(string key) => Get(key).Split(',', StringSplitOptions.TrimEntries);
    private static string RemoveSpaces(string value) => value.Replace(" ", string.Empty);
    private static string NumericOnly(string value)
        => RegexUtil.NumericalPattern().Replace(value.Trim(), string.Empty);
    private static string NumericOnly2(string value)
        => RegexUtil.NumericalPattern2().Replace(value, string.Empty);

    private string SacrificeValue => Get(Resources.Resources.General070_ReqSacrifice);
    private int SacrificeIdx => SacrificeValue.IndexOf(" x", StringComparison.Ordinal);

    internal string Quality => NumericOnly(Get(Resources.Resources.General035_Quality));
    internal string Level => NumericOnly(Get(Resources.Resources.General031_Lv).Trim());
    internal string ItemLevel => NumericOnly(Get(Resources.Resources.General032_ItemLv).Trim());
    internal string MapTier => RemoveSpaces(Get(Resources.Resources.General034_MaTier));
    internal string WaystoneTier => RemoveSpaces(Get(Resources.Resources.General143_WaystoneTier));
    internal string AreaLevel => RemoveSpaces(Get(Resources.Resources.General067_AreaLevel));
    internal string AreaLevelBis => RemoveSpaces(Get(Resources.Resources.General198_AreaLevelBis));
    internal string Socket => Get(Resources.Resources.General036_Socket);
    internal string Armour => NumericOnly(Get(Resources.Resources.General055_Armour));
    internal string Energy => NumericOnly(Get(Resources.Resources.General056_Energy));
    internal string Evasion => NumericOnly(Get(Resources.Resources.General057_Evasion));
    internal string Ward => NumericOnly(Get(Resources.Resources.General095_Ward));
    internal string PhysicalDamage => Get(Resources.Resources.General058_PhysicalDamage);
    internal string ElementalDamage => Get(Resources.Resources.General059_ElementalDamage);
    internal string ChaosDamage => Get(Resources.Resources.General060_ChaosDamage);
    internal string ColdDamage => Get(Resources.Resources.General148_ColdDamage);
    internal string FireDamage => Get(Resources.Resources.General149_FireDamage);
    internal string LightningDamage => Get(Resources.Resources.General146_LightningDamage);
    internal string AttacksPerSecond => NumericOnly2(Get(Resources.Resources.General061_AttacksPerSecond));
    internal string CriticalHitChance => Get(Resources.Resources.General147_CriticalHitChance);
    internal string StoredExperience => Get(Resources.Resources.General196_StoredExperience).Replace(" ", string.Empty).Replace(" ", string.Empty);
    internal string SacrificeItem => SacrificeIdx > -1 ? SacrificeValue[..SacrificeIdx] : string.Empty;
    internal string SacrificeCount => SacrificeIdx > -1 ? NumericOnly(SacrificeValue[SacrificeIdx..]) : string.Empty;
    internal string Reward => Get(Resources.Resources.General071_Reward);
    internal string[] Resolve => Get(Resources.Resources.General114_SanctumResolve).Split(' ')[0]
        .Split('/', StringSplitOptions.TrimEntries);
    internal string Inspiration => Get(Resources.Resources.General115_SanctumInspiration);
    internal string Aureus => Get(Resources.Resources.General116_SanctumAureus);
    internal string MinorBoons => Get(Resources.Resources.General117_SanctumMinorBoons);
    internal string[] MajorBoons => SplitComma(Resources.Resources.General118_SanctumMajorBoons);
    internal string[] MinorAfflictions => SplitComma(Resources.Resources.General119_SanctumMinorAfflictions);
    internal string[] MajorAfflictions => SplitComma(Resources.Resources.General120_SanctumMajorAfflictions);
    internal string[] Pacts => SplitComma(Resources.Resources.General123_SanctumPacts);
    internal string[] RewardsFloor => SplitComma(Resources.Resources.General121_RewardsFloorCompletion);
    internal string[] RewardsSanctum => SplitComma(Resources.Resources.General122_RewardsSanctumCompletion);
    internal string Monster => Get(Resources.Resources.General128_Monster);
    internal string CorpseLevel => RemoveSpaces(Get(Resources.Resources.General129_CorpseLevel));
    internal string MonsterCategory => Get(Resources.Resources.General130_MonsterCategory);
    internal string ItemQuantity => RemoveSpaces(Get(Resources.Resources.General136_ItemQuantity));
    internal string ItemRarity => RemoveSpaces(Get(Resources.Resources.General137_ItemRarity));
    internal string MonsterPackSize => RemoveSpaces(Get(Resources.Resources.General138_MonsterPackSize));
    internal string MoreCurrency => RemoveSpaces(Get(Resources.Resources.General139_MoreCurrency));
    internal string MoreScarabs => RemoveSpaces(Get(Resources.Resources.General140_MoreScarabs));
    internal string MoreMaps => RemoveSpaces(Get(Resources.Resources.General141_MoreMaps));
    internal string MoreDiv => RemoveSpaces(Get(Resources.Resources.General142_MoreDivinationCards));
    internal string RareMonsters => RemoveSpaces(Get(Resources.Resources.General162_RareMonsters));
    internal string MagicMonsters => RemoveSpaces(Get(Resources.Resources.General161_MagicMonsters));
    internal string Requires => NumericOnly(Get(Resources.Resources.General155_Requires).Split(',')[0]);
    internal string MemoryStrands => Get(Resources.Resources.General156_MemoryStrands);

    /// <summary>
    /// Update option dictionary.
    /// Return true to skip mod parsing.
    /// </summary>
    internal bool Update(ReadOnlySpan<char> data)
    {
        int idx = data.IndexOf(':');
        var keySpan = idx < 0 ? data.Trim() : data[..idx].Trim();
        var valueSpan = idx < 0 ? [] : data[(idx + 1)..].Trim();
        if (keySpan.Contain(Resources.Resources.General110_FoilUnique))
        {
            keySpan = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }
        else if (keySpan.StartWith(Resources.Resources.General035_Quality))
        {
            keySpan = Resources.Resources.General035_Quality; // Ignore catalyst quality type
        }

        var keyStr = keySpan.ToString();
        if (_options.TryGetValue(keyStr, out string value))
        {
            if (value.Length is 0)
            {
                _options[keyStr] = valueSpan.ToString();
            }
            return true;
        }
        return false;
    }
}
