using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class ItemOption
{
    private readonly Dictionary<string, string> _options = new()
    {
        { Resources.Resources.General035_Quality, string.Empty },
        { Resources.Resources.General031_Lv, string.Empty },
        { Resources.Resources.General032_ItemLv, string.Empty },
        { Resources.Resources.General034_MaTier, string.Empty },
        { Resources.Resources.General143_WaystoneTier, string.Empty },
        { Resources.Resources.General067_AreaLevel, string.Empty },
        { Resources.Resources.General036_Socket, string.Empty },
        { Resources.Resources.General055_Armour, string.Empty },
        { Resources.Resources.General056_Energy, string.Empty },
        { Resources.Resources.General057_Evasion, string.Empty },
        { Resources.Resources.General095_Ward, string.Empty },
        { Resources.Resources.General058_PhysicalDamage, string.Empty },
        { Resources.Resources.General059_ElementalDamage, string.Empty },
        { Resources.Resources.General060_ChaosDamage, string.Empty },
        { Resources.Resources.General148_ColdDamage, string.Empty },
        { Resources.Resources.General149_FireDamage, string.Empty },
        { Resources.Resources.General146_LightningDamage, string.Empty },
        { Resources.Resources.General061_AttacksPerSecond, string.Empty },
        { Resources.Resources.General147_CriticalHitChance, string.Empty },
        { Resources.Resources.Main154_tbFacetor, string.Empty },
        { Resources.Resources.General070_ReqSacrifice, string.Empty },
        { Resources.Resources.General071_Reward, string.Empty },
        { Resources.Resources.General114_SanctumResolve, string.Empty },
        { Resources.Resources.General115_SanctumInspiration, string.Empty },
        { Resources.Resources.General116_SanctumAureus, string.Empty },
        { Resources.Resources.General117_SanctumMinorBoons, string.Empty },
        { Resources.Resources.General118_SanctumMajorBoons, string.Empty },
        { Resources.Resources.General119_SanctumMinorAfflictions, string.Empty },
        { Resources.Resources.General120_SanctumMajorAfflictions, string.Empty },
        { Resources.Resources.General123_SanctumPacts, string.Empty },
        { Resources.Resources.General121_RewardsFloorCompletion, string.Empty },
        { Resources.Resources.General122_RewardsSanctumCompletion, string.Empty },
        { Resources.Resources.General128_Monster, string.Empty },
        { Resources.Resources.General129_CorpseLevel, string.Empty },
        { Resources.Resources.General130_MonsterCategory, string.Empty },
        { Resources.Resources.General136_ItemQuantity, string.Empty },
        { Resources.Resources.General137_ItemRarity, string.Empty },
        { Resources.Resources.General138_MonsterPackSize, string.Empty },
        { Resources.Resources.General139_MoreCurrency, string.Empty },
        { Resources.Resources.General140_MoreScarabs, string.Empty },
        { Resources.Resources.General141_MoreMaps, string.Empty },
        { Resources.Resources.General142_MoreDivinationCards, string.Empty },
        { Resources.Resources.General162_RareMonsters, string.Empty },
        { Resources.Resources.General161_MagicMonsters, string.Empty },
        { Resources.Resources.General155_Requires, string.Empty },
        { Resources.Resources.General156_MemoryStrands, string.Empty },
    };

    private int SacrificeIdx => _options[Resources.Resources.General070_ReqSacrifice].IndexOf(" x", StringComparison.Ordinal);

    internal string Quality => RegexUtil.NumericalPattern().
        Replace(_options[Resources.Resources.General035_Quality].Trim(), string.Empty);
    internal string Level => RegexUtil.NumericalPattern().
        Replace(_options[Resources.Resources.General031_Lv].Trim(), string.Empty);
    internal string ItemLevel => RegexUtil.NumericalPattern().
        Replace(_options[Resources.Resources.General032_ItemLv].Trim(), string.Empty);
    internal string MapTier => _options[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
    internal string WaystoneTier => _options[Resources.Resources.General143_WaystoneTier].Replace(" ", string.Empty); // 0x20;
    internal string AreaLevel => _options[Resources.Resources.General067_AreaLevel].Replace(" ", string.Empty);
    internal string Socket => _options[Resources.Resources.General036_Socket];
    internal string Armour => RegexUtil.NumericalPattern().Replace(_options[Resources.Resources.General055_Armour].Trim(), string.Empty);
    internal string Energy => RegexUtil.NumericalPattern().Replace(_options[Resources.Resources.General056_Energy].Trim(), string.Empty);
    internal string Evasion => RegexUtil.NumericalPattern().Replace(_options[Resources.Resources.General057_Evasion].Trim(), string.Empty);
    internal string Ward => RegexUtil.NumericalPattern().Replace(_options[Resources.Resources.General095_Ward].Trim(), string.Empty);
    internal string PhysicalDamage => _options[Resources.Resources.General058_PhysicalDamage];
    internal string ElementalDamage => _options[Resources.Resources.General059_ElementalDamage];
    internal string ChaosDamage => _options[Resources.Resources.General060_ChaosDamage];
    internal string ColdDamage => _options[Resources.Resources.General148_ColdDamage];
    internal string FireDamage => _options[Resources.Resources.General149_FireDamage];
    internal string LightningDamage => _options[Resources.Resources.General146_LightningDamage];
    internal string AttacksPerSecond => RegexUtil.NumericalPattern2().Replace(_options[Resources.Resources.General061_AttacksPerSecond], string.Empty);
    internal string CriticalHitChance => _options[Resources.Resources.General147_CriticalHitChance]; // not used
    internal string StoredExperience => _options[Resources.Resources.Main154_tbFacetor].Replace(" ", string.Empty);
    internal string SacrificeItem => SacrificeIdx is var idx && idx > -1 ? _options[Resources.Resources.General070_ReqSacrifice].AsSpan(0, idx).ToString() : string.Empty;
    internal string SacrificeCount => SacrificeIdx is var idx && idx > -1 ? RegexUtil.NumericalPattern()
        .Replace(_options[Resources.Resources.General070_ReqSacrifice].AsSpan(idx).ToString(), string.Empty) : string.Empty; // not called
    internal string Reward => _options[Resources.Resources.General071_Reward];
    internal string[] Resolve => _options[Resources.Resources.General114_SanctumResolve].Split(' ')[0].Split('/', StringSplitOptions.TrimEntries);
    internal string Inspiration => _options[Resources.Resources.General115_SanctumInspiration];
    internal string Aureus => _options[Resources.Resources.General116_SanctumAureus];
    internal string MinorBoons => _options[Resources.Resources.General117_SanctumMinorBoons]; // not called
    internal string[] MajorBoons => _options[Resources.Resources.General118_SanctumMajorBoons].Split(',', StringSplitOptions.TrimEntries);
    internal string[] MinorAfflictions => _options[Resources.Resources.General119_SanctumMinorAfflictions].Split(',', StringSplitOptions.TrimEntries); // not called
    internal string[] MajorAfflictions => _options[Resources.Resources.General120_SanctumMajorAfflictions].Split(',', StringSplitOptions.TrimEntries);
    internal string[] Pacts => _options[Resources.Resources.General123_SanctumPacts].Split(',', StringSplitOptions.TrimEntries);
    internal string[] RewardsFloor => _options[Resources.Resources.General121_RewardsFloorCompletion].Split(',', StringSplitOptions.TrimEntries);
    internal string[] RewardsSanctum => _options[Resources.Resources.General122_RewardsSanctumCompletion].Split(',', StringSplitOptions.TrimEntries);
    internal string Monster => _options[Resources.Resources.General128_Monster]; // not called
    internal string CorpseLevel => _options[Resources.Resources.General129_CorpseLevel].Replace(" ", string.Empty);
    internal string MonsterCategory => _options[Resources.Resources.General130_MonsterCategory]; // not called
    internal string ItemQuantity => _options[Resources.Resources.General136_ItemQuantity].Replace(" ", string.Empty);
    internal string ItemRarity => _options[Resources.Resources.General137_ItemRarity].Replace(" ", string.Empty);
    internal string MonsterPackSize => _options[Resources.Resources.General138_MonsterPackSize].Replace(" ", string.Empty);
    internal string MoreCurrency => _options[Resources.Resources.General139_MoreCurrency].Replace(" ", string.Empty);
    internal string MoreScarabs => _options[Resources.Resources.General140_MoreScarabs].Replace(" ", string.Empty);
    internal string MoreMaps => _options[Resources.Resources.General141_MoreMaps].Replace(" ", string.Empty);
    internal string MoreDiv => _options[Resources.Resources.General142_MoreDivinationCards].Replace(" ", string.Empty);
    internal string RareMonsters => _options[Resources.Resources.General162_RareMonsters].Replace(" ", string.Empty);
    internal string MagicMonsters => _options[Resources.Resources.General161_MagicMonsters].Replace(" ", string.Empty);
    internal string Requires => RegexUtil.NumericalPattern().Replace(_options[Resources.Resources.General155_Requires].Split(',')[0].Trim(), string.Empty);
    internal string MemoryStrands => _options[Resources.Resources.General156_MemoryStrands];

    internal ItemOption()
    {

    }

    /// <summary>
    /// Update Option dictionnary.
    /// </summary>
    /// <remarks>
    /// Return 'true' to skip mod parsing or 'false' to proceed.
    /// </remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    internal bool Update(ReadOnlySpan<char> data)
    {
        int idx = data.IndexOf(':');
        var keySpan = idx is -1 ? data.Trim() : data[..idx].Trim();
        var valueSpan = idx is -1 ? [] : data[(idx + 1)..].Trim();

        if (keySpan.Contain(Resources.Resources.General110_FoilUnique))
        {
            keySpan = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }
        if (keySpan.StartWith(Resources.Resources.General035_Quality))
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
