namespace Xiletrade.Library.Shared.Enum;

/// <summary>
/// Item flags usable in the 'when' conditions of the stat rules (StatRules.json / StatRulesTwo.json).
/// </summary>
/// <remarks>Name used in the files : see StatRuleExtensions.ParseFlag, the only place where they are written.</remarks>
internal enum StatRuleFlag : byte
{
    None = 0,
    Weapon,
    Armour,
    Unique,
    Jewel,
    Cluster,
    Tincture,
    Charm,
    Flask,
    Amulet,
    Shield,
    Waystone,
    Tablet,
    Chronicle,
    Logbook,
    SanctumRelic
}
