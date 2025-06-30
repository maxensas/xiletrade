using System;
using System.Globalization;
using System.Text;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed record ItemDamage
{
    private readonly Lang _lang;

    internal double Total { get; }
    internal string TotalMin { get; } = string.Empty;
    internal string TotalString { get; } = string.Empty;
    internal string PysicalMin { get; } = string.Empty;
    internal string ElementalMin { get; } = string.Empty;
    internal string Tip { get; } = string.Empty;

    internal ItemDamage(ItemData item, string item_quality)
    {
        _lang = item.Lang;        
        string specifier = "G";
        double qualityDPS = item_quality.ToDoubleDefault();
        double physicalDPS = DamageToDPS(item.Option[Resources.Resources.General058_PhysicalDamage]);
        double elementalDPS = DamageToDPS(item.Option[Resources.Resources.General059_ElementalDamage])
            + DamageToDPS(item.Option[Resources.Resources.General148_ColdDamage])
            + DamageToDPS(item.Option[Resources.Resources.General149_FireDamage])
            + DamageToDPS(item.Option[Resources.Resources.General146_LightningDamage]);
        double chaosDPS = DamageToDPS(item.Option[Resources.Resources.General060_ChaosDamage]);
        string aps = RegexUtil.NumericalPattern2().Replace(item.Option[Resources.Resources.General061_AttacksPerSecond], string.Empty);

        double attacksPerSecond = aps.ToDoubleDefault();

        physicalDPS = physicalDPS / 2 * attacksPerSecond;
        if (qualityDPS < 20 && !item.Flag.Corrupted)
        {
            double physInc = item.TotalIncPhys;
            double physMulti = (physInc + qualityDPS + 100) / 100;
            double basePhys = physicalDPS / physMulti;
            physicalDPS = basePhys * ((physInc + 120) / 100);
        }
        elementalDPS = elementalDPS / 2 * attacksPerSecond;
        chaosDPS = chaosDPS / 2 * attacksPerSecond;

        // remove values after decimal to avoid difference with POE's rounded values while calculating dps weapons
        physicalDPS = Math.Truncate(physicalDPS);
        elementalDPS = Math.Truncate(elementalDPS);
        chaosDPS = Math.Truncate(chaosDPS);
        Total = physicalDPS + elementalDPS + chaosDPS;
        TotalString = Math.Round(Total, 0).ToString() + " DPS";

        StringBuilder sbToolTip = new();

        // Allready rounded : example 0.46 => 0.5
        TotalMin = Total.ToString(specifier, CultureInfo.InvariantCulture);

        if (Math.Round(physicalDPS, 2) > 0)
        {
            string qual = qualityDPS > 20 || item.Flag.Corrupted ? qualityDPS.ToString() : "20";
            sbToolTip.Append("PHYS. Q").Append(qual).Append(" : ").Append(Math.Round(physicalDPS, 0)).Append(" dps");

            PysicalMin = Math.Round(physicalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (Math.Round(elementalDPS, 2) > 0)
        {
            if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
            sbToolTip.Append("ELEMENTAL : ").Append(Math.Round(elementalDPS, 0)).Append(" dps");

            ElementalMin = Math.Round(elementalDPS, 0).ToString(specifier, CultureInfo.InvariantCulture);
        }
        if (Math.Round(chaosDPS, 2) > 0)
        {
            if (sbToolTip.ToString().Length > 0) sbToolTip.AppendLine();
            sbToolTip.Append("CHAOS : ").Append(Math.Round(chaosDPS, 0)).Append(" dps");
        }
        Tip = sbToolTip.ToString();
    }

    private double DamageToDPS(string damage)
    {
        double dps = 0;
        try
        {
            if (_lang is Lang.Taiwanese)
            {
                damage = damage.Replace('到', '-').Replace(" ", string.Empty);
            }
            var stmps = RegexUtil.LetterPattern().Replace(damage, string.Empty).Split(',');
            for (int t = 0; t < stmps.Length; t++)
            {
                var maidps = (stmps[t] ?? string.Empty).Trim().Split('-');
                if (maidps.Length is 2)
                {
                    double min = double.Parse(maidps[0].Trim());
                    double max = double.Parse(maidps[1].Trim());

                    dps += min + max;
                }
            }
        }
        catch (Exception)
        {
            //Shared.Util.Helper.Debug.Trace("Exception while calculating DPS : " + ex.Message);
        }
        return dps;
    }
}
