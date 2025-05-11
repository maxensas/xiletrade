using System.Collections.Generic;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

public sealed class MinMaxModel(StatPanel stat, string text)
{
    public StatPanel Id { get; set; } = stat;

    public string Text { get; set; } = text;

    public string Min { get; set; } = string.Empty;

    public string Max { get; set; } = string.Empty;

    public double MinSlide { get; set; } = ModFilter.EMPTYFIELD;

    public double MinSlideDefault { get; set; } = ModFilter.EMPTYFIELD;

    public bool Selected { get; set; } = false;

    public bool ShowSlide { get; set; }

    //public bool Visible { get; set; }

    public void UpdateMinSlide()
    {
        if (Min.Length is 0 || Max.Length > 0)
        {
            ShowSlide = false;
            return;
        }
        MinSlide = MinSlideDefault = Min.ToDoubleEmptyField();
        ShowSlide = true;
        return;
    }

    public static IEnumerable<MinMaxModel> GetNewMinMaxList()
    {
        var list = new List<MinMaxModel>();

        list.Add(new (StatPanel.CommonItemLevel,""));
        list.Add(new (StatPanel.CommonQuality, Resources.Resources.Main066_tbQuality));
        list.Add(new (StatPanel.CommonSocket, Resources.Resources.General036_Socket));
        list.Add(new (StatPanel.CommonLink, Resources.Resources.General154_Links));
        list.Add(new (StatPanel.CommonSocketRune, Resources.Resources.Main228_tbRuneSocketsTip));
        list.Add(new (StatPanel.DamagePhysical, Resources.Resources.Main074_tbPhysDps));
        list.Add(new (StatPanel.DamageElemental, Resources.Resources.Main075_tbElemDps));
        list.Add(new (StatPanel.DamageTotal, Resources.Resources.Main073_tbTotalDps));
        list.Add(new (StatPanel.DefenseArmour, Resources.Resources.Main068_tbArmour));
        list.Add(new (StatPanel.DefenseEnergy, Resources.Resources.Main069_tbEnergy));
        list.Add(new (StatPanel.DefenseEvasion, Resources.Resources.Main070_tbEvasion));
        list.Add(new (StatPanel.DefenseWard, Resources.Resources.General095_Ward));
        list.Add(new (StatPanel.TotalResistance, Resources.Resources.Main076_tbTotalResist));
        list.Add(new (StatPanel.TotalLife, Resources.Resources.Main077_tbTotalLife));
        list.Add(new (StatPanel.TotalGlobalEs, Resources.Resources.Main078_tbGlobalES));
        list.Add(new (StatPanel.MapQuantity, Resources.Resources.General133_Iiq));
        list.Add(new (StatPanel.MapRarity, Resources.Resources.General134_Iir));
        list.Add(new (StatPanel.MapPackSize, Resources.Resources.General135_PackSize));
        list.Add(new (StatPanel.MapMoreScarab, Resources.Resources.General140_MoreScarabs));
        list.Add(new (StatPanel.MapMoreCurrency, Resources.Resources.General139_MoreCurrency));
        list.Add(new (StatPanel.MapMoreDivCard, Resources.Resources.General142_MoreDivinationCards));
        list.Add(new (StatPanel.MapMoreMap, Resources.Resources.General141_MoreMaps));
        list.Add(new (StatPanel.SanctumResolve, Resources.Resources.General114_SanctumResolve));
        list.Add(new (StatPanel.SanctumMaxResolve, Resources.Resources.General124_SanctumMaxResolve));
        list.Add(new (StatPanel.SanctumInspiration, Resources.Resources.General115_SanctumInspiration));
        list.Add(new (StatPanel.SanctumAureus, Resources.Resources.General116_SanctumAureus));

        return list;
    }
}
