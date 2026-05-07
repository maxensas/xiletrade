using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

// WIP : TODO create and parse all members here properly instead of doing it in other models/viewmodels.
internal sealed class ItemOption
{
    internal Dictionary<string, string> Option { get; } = InitListOption();

    internal string Quality => RegexUtil.NumericalPattern().
        Replace(Option[Resources.Resources.General035_Quality].Trim(), string.Empty);
    internal string MapTier => Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);

    internal ItemOption()
    {

    }

    private static Dictionary<string, string> InitListOption()
    {
        return new Dictionary<string, string>()
        {
            { Resources.Resources.General035_Quality, string.Empty },
            { Resources.Resources.General031_Lv, string.Empty },
            { Resources.Resources.General032_ItemLv, string.Empty },
            { Resources.Resources.General033_TalTier, string.Empty },
            { Resources.Resources.General034_MaTier, string.Empty },
            { Resources.Resources.General067_AreaLevel, string.Empty },
            { Resources.Resources.General036_Socket, string.Empty },
            { Resources.Resources.General055_Armour, string.Empty },
            { Resources.Resources.General056_Energy, string.Empty },
            { Resources.Resources.General057_Evasion, string.Empty },
            { Resources.Resources.General095_Ward, string.Empty },
            { Resources.Resources.General058_PhysicalDamage, string.Empty },
            { Resources.Resources.General059_ElementalDamage, string.Empty },
            { Resources.Resources.General060_ChaosDamage, string.Empty },
            { Resources.Resources.General061_AttacksPerSecond, string.Empty },
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
            { Resources.Resources.General143_WaystoneTier, string.Empty },
            { Resources.Resources.General146_LightningDamage, string.Empty },
            { Resources.Resources.General147_CriticalHitChance, string.Empty },
            { Resources.Resources.General148_ColdDamage, string.Empty },
            { Resources.Resources.General149_FireDamage, string.Empty },
            { Resources.Resources.General155_Requires, string.Empty },
            { Resources.Resources.General156_MemoryStrands, string.Empty },
        };
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
        ReadOnlySpan<char> keySpan = idx is -1 ? data.Trim() : data[..idx].Trim();
        ReadOnlySpan<char> valueSpan = idx is -1 ? [] : data[(idx + 1)..].Trim();

        if (keySpan.Contain(Resources.Resources.General110_FoilUnique))
        {
            keySpan = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }
        if (keySpan.StartWith(Resources.Resources.General035_Quality))
        {
            keySpan = Resources.Resources.General035_Quality; // Ignore catalyst quality type
        }

        var keyStr = keySpan.ToString();
        if (Option.TryGetValue(keyStr, out string value))
        {
            if (value.Length is 0)
            {
                var isSocket = keyStr == Resources.Resources.General036_Socket;
                var minLength = isSocket ? 1 : 2;
                Option[keyStr] = valueSpan.Length >= minLength ? valueSpan.ToString() : Strings.TrueOption;
            }
            return true;
        }

        return false;
    }
}
