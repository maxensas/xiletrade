using System.Collections.Generic;

namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class StatPoe1
    {
        internal static class Influence
        {
            internal const string Shaper = "pseudo_has_shaper_influence"; // Has Shaper Influence
            internal const string Elder = "pseudo_has_elder_influence"; // Has Elder Influence
            internal const string Crusader = "pseudo_has_crusader_influence"; // Has Crusader Influence
            internal const string Redeemer = "pseudo_has_redeemer_influence"; // Has Redeemer Influence
            internal const string Hunter = "pseudo_has_hunter_influence"; // Has Hunter Influence
            internal const string Warlord = "pseudo_has_warlord_influence"; // Has Warlord Influence
            internal const string Count = "pseudo_has_influence_count"; // Has # Influences
        }

        internal static class Temple
        {
            internal const string Room01 = "pseudo_temple_apex"; // Apex of Atzoatl
            internal const string Room11 = "pseudo_temple_gem_room_3"; // Doryani's Institute
            internal const string Room15 = "pseudo_temple_sacrifice_room_3"; // Apex of Ascension
            internal const string Room17 = "pseudo_temple_corruption_room_3"; // Locus of Corruption
        }

        internal static class Lake
        {
            internal const string Tablet01 = "pseudo.lake_50846"; // Reflection of Paradise (Difficulty #)
            internal const string Tablet02 = "pseudo.lake_36591"; // Reflection of Kalandra (Difficulty #)
            internal const string Tablet03 = "pseudo.lake_60034"; // Reflection of the Sun (Difficulty #)
            internal const string Tablet04 = "pseudo.lake_40794"; // Reflection of Angling (Difficulty #)
        }

        internal static class Pseudo
        {
            internal const string TotalElemResistance = "pseudo.pseudo_total_elemental_resistance"; // +#% total Elemental Resistance
            internal const string TotalLife = "pseudo.pseudo_total_life"; // +# total maximum Life
            internal const string TotalEs = "pseudo.pseudo_total_energy_shield"; // # to maximum Energy Shield
            internal const string TotalAttribute = "pseudo.pseudo_total_attributes"; // +# total to Attributes // ONLY POE2
            internal const string MoreScarab = "pseudo.pseudo_map_more_scarab_drops"; // More Scarabs: #%
            internal const string MoreCurrency = "pseudo.pseudo_map_more_currency_drops"; // More Currency: #%
            internal const string MoreDivCard = "pseudo.pseudo_map_more_card_drops"; // More Divination Cards: #%

            internal const string EmmptyPrefix = "pseudo.pseudo_number_of_empty_prefix_mods"; // # Empty Prefix Modifiers
            internal const string EmptySuffix = "pseudo.pseudo_number_of_empty_suffix_mods"; // # Empty Suffix Modifiers
        }

        internal static class Generic
        {
            internal const string UseRemaining = "stat_1479533453"; // # use remaining : enchant.stat_290368246 / explicit.stat_1479533453
            internal const string PassiveSkill = "stat_3086156145"; // Adds # Passive Skills
            internal const string PassiveJewel = "stat_4079888060"; // # Added Passive Skills are Jewel Sockets
            internal const string GrantNothing = "stat_1085446536"; // Adds # Small Passive Skills which grant nothing
            internal const string Crafted = "stat_1859333175"; // Can have up to 3 Crafted Modifiers
            internal const string IncPhys = "stat_1509134228"; // #% increased Physical Damage

            internal const string LogbookBoss = "stat_3159649981"; // Area contains an Expedition Boss (#)
            internal const string LogbookArea = "stat_1160596338"; // Area contains an additional Underground Area
            internal const string LogbookTwice = "stat_3239978999"; // Excavated Chests have a #% chance to contain twice as many Items

            //non-local
            internal const string BlockStaff = "stat_1778298516"; // #% Chance to Block Attack Damage while wielding a Staff

            internal const string AddArmor = "stat_809229260"; // # to Armour
            internal const string AddEs = "stat_3489782002"; // # to maximum Energy Shield
            internal const string AddEva = "stat_2144192055"; // # to Evasion Rating

            //local
            internal const string Block = "stat_4253454700"; // #% Chance to Block (Shields)
            internal const string BlockStaffWeapon = "stat_1001829678"; // #% Chance to Block Attack Damage while wielding a Staff (Staves)

            internal const string AddArmorFlat = "stat_3484657501"; // # to Armour (Local)
            internal const string AddEsFlat = "stat_4052037485"; // # to maximum Energy Shield (Local)
            internal const string AddEvaFlat = "stat_53045048"; // # to Evasion Rating (Local)

            internal const string IncEs = "stat_4015621042"; // #% increased Energy Shield (Local)
            internal const string IncEva = "stat_124859000"; // #% increased Evasion Rating (Local)
            internal const string IncArmour = "stat_1062208444"; // #% increased Armour (Local)
            internal const string IncAe = "stat_2451402625"; // #% increased Armour and Evasion (Local)
            internal const string IncAes = "stat_3321629045"; // #% increased Armour and Energy Shield (Local)
            internal const string IncEes = "stat_1999113824"; // #% increased Evasion and Energy Shield (Local)
            internal const string IncArEes = "stat_3523867985"; // #% increased Armour, Evasion and Energy Shield (Local)

            internal const string AddAccuracyLocal = "stat_691932474"; // # to Accuracy Rating (Local)
            internal const string LifeLeech = "stat_55876295"; // #% of Physical Attack Damage Leeched as Life (Local)
            internal const string ManaLeech = "stat_669069897"; // #% of Physical Attack Damage Leeched as Mana (Local)
            internal const string PoisonHit = "stat_3885634897"; // #% chance to Poison on Hit (Local)
            internal const string AttackSpeed = "stat_210067635"; // #% increased Attack Speed (Local)
            internal const string IncPhysFlat = "stat_1940865751"; // Adds # to # Physical Damage (Local)
            internal const string IncLightFlat = "stat_3336890334"; // Adds # to # Lightning Damage (Local)
            internal const string IncColdFlat = "stat_1037193709"; // Adds # to # Cold Damage (Local)
            internal const string IncFireFlat = "stat_709508406"; // Adds # to # Fire Damage (Local)
            internal const string IncChaosFlat = "stat_2223678961"; // Adds # to # Chaos Damage (Local)
        }

        internal static class Option
        {
            internal const string SmallClusterPassive = "enchant.stat_3948993189"; // Added Small Passive Skills grant: #
            internal const string MapOccupConq = "implicit.stat_2563183002"; // Map contains #'s Citadel\nItem Quantity increases amount of Rewards # drops by 20% of its value
            internal const string MapOccupElder = "implicit.stat_3624393862"; // Map is occupied by #
            internal const string AreaInflu = "implicit.stat_1792283443"; // Area is influenced by #
        }

        internal static class Map
        {
            internal static readonly (string Id, string Name) Enslaver = ("implicit.stat_3624393862|1", "The Enslaver"); // Map is occupied by The Enslaver
            internal static readonly (string Id, string Name) Eradicator = ("implicit.stat_3624393862|2", "The Eradicator"); // Map is occupied by The Eradicator
            internal static readonly (string Id, string Name) Constrictor = ("implicit.stat_3624393862|3", "The Constrictor"); // Map is occupied by The Constrictor
            internal static readonly (string Id, string Name) Purifier = ("implicit.stat_3624393862|4", "The Purifier"); // Map is occupied by The Purifier

            internal static readonly (string Id, string Name) Baran = ("implicit.stat_2563183002|1", "Baran"); // Map contains Baran's Citadel\n...
            internal static readonly (string Id, string Name) Veritania = ("implicit.stat_2563183002|2", "Veritania"); // Map contains Veritania's Citadel\n...
            internal static readonly (string Id, string Name) AlHezmin = ("implicit.stat_2563183002|3", "Al-Hezmin"); // Map contains Al-Hezmin's Citadel\n...
            internal static readonly (string Id, string Name) Drox = ("implicit.stat_2563183002|4", "Drox"); // Map contains Drox's Citadel\n...
        }

        //implicits
        internal const string ActionSpeed = "implicit.stat_2878959938"; // #% reduced Action Speed
        internal const string AreaInfluOrigin = "implicit.stat_2696470877"; // Area is Influenced by the Originator's Memories

        //explicits
        internal const string StunOnYou = "explicit.stat_1067429236"; // #% increased Stun Duration on you

        internal const string IncExpGain = "explicit.stat_3666934677"; // #% increased Experience gain
        internal const string IncExpGainMap = "explicit.stat_57434274"; // #% increased Experience gain (Maps)

        internal const string ImmunityIgnite2 = "explicit.stat_2695527599"; // Grants Immunity to Ignite for 4 seconds if used while Ignited\nRemoves all Burning when used

        internal const string MonsterLifeOld = "explicit.stat_95249895";
        internal const string MonsterLifeNew = "explicit.stat_2710898947";

        internal const string TimelessJewel = "explicit.pseudo_timeless_jewel";

        internal const string Conflux = "explicit.stat_1190121450";

        internal const string BleedingAvoid = "explicit.stat_1618589784"; // #% chance to Avoid Bleeding
        internal const string BleedingCannot = "explicit.stat_1901158930"; // Bleeding cannot be inflicted on you

        internal const string SocketsUnmodifiable = "explicit.stat_3192592092"; // Sockets cannot be modified

                                                                                 //internal const string SocketedPierce3 = "explicit.stat_2433615566"; // Socketed Gems are supported by Level # Pierce

        //local

        //non-local

        //veiled
        internal const string VeiledPrefix = "veiled.mod_65000"; // Veiled
        internal const string VeiledSuffix = "veiled.mod_63099"; // of the Veil

        internal const string MaxLife = "explicit.stat_3299347043"; // +# to maximum Life
        internal const string FireResist = "explicit.stat_3372524247"; // +#% to Fire Resistance
        internal const string ColdResist = "explicit.stat_4220027924"; // +#% to Cold Resistance
        internal const string LightningResist = "explicit.stat_1671376347"; // +#% to Lightning Resistance

        /*
        internal const string TotalResistance = "+#% total Elemental Resistance";

        internal const string FlatPhysicalDamage = "Adds # to # Physical Damage";
        internal const string FlatAccuracyRating = "# to Accuracy Rating";
        internal const string FlatColdDamage = "Adds # to # Cold Damage";
        internal const string FlatLightningDamage = "Adds # to # Lightning Damage";
        internal const string FlatFireDamage = "Adds # to # Fire Damage";
        internal const string FlatChaosDamage = "Adds # to # Chaos Damage";

        internal const string EnergyShield = "#% increased Energy Shield";
        internal const string EvasionRating = "#% increased Evasion Rating";
        internal const string ArmourRating = "#% increased Armour";

        internal const string FlatEnergyShield = "# to maximum Energy Shield";
        internal const string FlatEvasionRating = "# to Evasion Rating";
        internal const string FlatArmourRating = "# to Armour";

        internal const string ArmourEvasionRating = "#% increased Armour and Evasion";
        internal const string ArmourEnergyShield = "#% increased Armour and Energy Shield";
        internal const string EvasionEnergyShield = "#% increased Evasion and Energy Shield";
        internal const string ArmourEvasionEnergyShield = "#% increased Armour, Evasion and Energy Shield";
        */

        internal static readonly Dictionary<string, string> dicPseudo = new()
        {
            { "stat_4220027924", "pseudo_total_cold_resistance" }, { "stat_3372524247", "pseudo_total_fire_resistance" }, { "stat_1671376347", "pseudo_total_lightning_resistance" }, { "stat_2923486259", "pseudo_total_chaos_resistance" },
            { "stat_3299347043", "pseudo_total_life" }, { "stat_1050105434", "pseudo_total_mana" }, { "stat_3489782002", "pseudo_total_energy_shield" }, { "stat_2482852589", "pseudo_increased_energy_shield" },
            { "stat_4080418644", "pseudo_total_strength" }, { "stat_3261801346", "pseudo_total_dexterity" }, { "stat_328541901", "pseudo_total_intelligence" },
            { "stat_681332047", "pseudo_total_attack_speed" }, { "stat_2891184298", "pseudo_total_cast_speed" }, { "stat_2250533757", "pseudo_increased_movement_speed" },
            { "stat_587431675", "pseudo_global_critical_strike_chance" }, { "stat_3556824919", "pseudo_global_critical_strike_multiplier" }, { "stat_737908626", "pseudo_critical_strike_chance_for_spells" },
            { "stat_1509134228", "pseudo_increased_physical_damage" }, { "stat_2974417149", "pseudo_increased_spell_damage" }, { "stat_3141070085", "pseudo_increased_elemental_damage" },
            { "stat_2231156303", "pseudo_increased_lightning_damage" }, { "stat_3291658075", "pseudo_increased_cold_damage" }, { "stat_3962278098", "pseudo_increased_fire_damage" },
            { "stat_4208907162", "pseudo_increased_lightning_damage_with_attack_skills" }, { "stat_860668586", "pseudo_increased_cold_damage_with_attack_skills" }, { "stat_2468413380", "pseudo_increased_fire_damage_with_attack_skills" }, { "stat_387439868", "pseudo_increased_elemental_damage_with_attack_skills" },
            { "stat_960081730", "pseudo_adds_physical_damage" }, { "stat_1334060246", "pseudo_adds_lightning_damage" }, { "stat_2387423236", "pseudo_adds_cold_damage" }, { "stat_321077055", "pseudo_adds_fire_damage" }, { "stat_3531280422", "pseudo_adds_chaos_damage" },
            { "stat_3032590688", "pseudo_adds_physical_damage_to_attacks" }, { "stat_1754445556", "pseudo_adds_lightning_damage_to_attacks" }, { "stat_4067062424", "pseudo_adds_cold_damage_to_attacks" }, { "stat_1573130764", "pseudo_adds_fire_damage_to_attacks" }, { "stat_674553446", "pseudo_adds_chaos_damage_to_attacks" },
            { "stat_2435536961", "pseudo_adds_physical_damage_to_spells" }, { "stat_2831165374", "pseudo_adds_lightning_damage_to_spells" }, { "stat_2469416729", "pseudo_adds_cold_damage_to_spells" }, { "stat_1133016593", "pseudo_adds_fire_damage_to_spells" }, { "stat_2300399854", "pseudo_adds_chaos_damage_to_spells" },
            { "stat_3325883026", "pseudo_total_life_regen" }, { "stat_836936635", "pseudo_percent_life_regen" }, { "stat_789117908", "pseudo_increased_mana_regen" }
        };

        internal static readonly string[] lSpecialImplicits =
        [
            "implicit.stat_227523295", // # to Maximum Power Charges
            "implicit.stat_1515657623", // # to Maximum Endurance Charges
            "implicit.stat_4078695", // # to Maximum Frenzy Charges
            "implicit.stat_3967845372", // Curse Enemies with Vulnerability on Hit, with #% increased Effect
            "implicit.stat_2028847114", // Curse Enemies with Elemental Weakness on Hit, with #% increased Effect
            "implicit.stat_4096052153", // Zealotry has #% increased Aura Effect
            "implicit.stat_4175197580", // Malevolence has #% increased Aura Effect
            "implicit.stat_2763429652", // #% chance to Maim on Hit
            "implicit.stat_3023957681", // #% chance to gain Onslaught for 4 seconds on Kill
            "implicit.stat_3433724931", // Curse Enemies with Temporal Chains on Hit, with #% increased Effect
            "implicit.stat_30642521", // You can apply # additional Curses
            "implicit.stat_1619454789", // Onslaught
            "implicit.stat_2264523604", // #% increased Reservation of Skills
            "implicit.stat_1658498488", // Corrupted Blood cannot be inflicted on you
            "implicit.stat_2843100721", // # to Level of Socketed Gems
            "implicit.stat_1592278124", // Anger has #% increased Aura Effect
            "implicit.stat_4247488219", // Pride has #% increased Aura Effect
            "implicit.stat_2495041954", // Overwhelm #% Physical Damage Reduction
            "implicit.stat_2551600084", // # to Level of Socketed AoE Gems
            "implicit.stat_2176571093", // # to Level of Socketed Projectile Gems
            "implicit.stat_2115168758", // # to Level of Socketed Duration Gems
            "implicit.stat_788317702", // Discipline has #% increased Aura Effect
            "implicit.stat_2452998583", // # to Level of Socketed Aura Gems
            "implicit.stat_2181791238", // Wrath has #% increased Aura Effect
            "implicit.stat_3742945352", // Hatred has #% increased Aura Effect
            "implicit.stat_397427740", // Grace has #% increased Aura Effect
            "implicit.stat_2067062068", // Projectiles Pierce # additional Targets
            "implicit.stat_3753703249", // Gain #% of Physical Damage as Extra Damage of a random Element
            "implicit.stat_452077019", // Slaying Enemies in a kill streak grants Rampage bonuses
            "implicit.stat_3814876985", // #% chance to gain a Power Charge on Critical Strike
            "implicit.stat_3943945975", // Resolute Technique
            "implicit.stat_742529963", // Bow Attacks fire # additional Arrows
            "implicit.stat_1172810729", // #% chance to deal Double Damage
            "implicit.stat_2524254339", // Culling Strike
            "implicit.stat_2896346114", // Point Blank
            "implicit.stat_369494213", // Gain #% of Physical Damage as Extra Fire Damage
            "implicit.stat_2429546158", // Grants Level # Hatred Skill
                                        //"implicit.stat_484879947", // Grants Level # Anger Skill
            "implicit.stat_74338099", // Skills fire an additional Projectile
            "implicit.stat_350598685", // # to Weapon Range
            "implicit.stat_979246511", // Gain #% of Physical Damage as Extra Cold Damage
            "implicit.stat_2192875806", // Socketed Skills apply Fire, Cold and Lightning Exposure on Hit
            "implicit.stat_219391121", // Gain #% of Physical Damage as Extra Lightning Damage
            "implicit.stat_3224664127", // Grants Level # Zealotry Skill
            "implicit.stat_2341269061", // Grants Level # Discipline Skill
            "implicit.stat_1181501418", // # to Maximum Rage
            "implicit.stat_3240769289", // #% of Physical Damage Converted to Lightning Damage
            "implicit.stat_1533563525", // #% of Physical Damage Converted to Fire Damage
            "implicit.stat_2133341901", // #% of Physical Damage Converted to Cold Damage
            "implicit.stat_338121249", // Curse Enemies with Flammability on Hit, with #% increased Effect
            "implicit.stat_4154259475", // # to Level of Socketed Support Gems
            "implicit.stat_1220361974", // Enemies you Kill Explode, dealing #% of their Life as Physical Damage
            "implicit.stat_1263158408", // Elemental Equilibrium
            "implicit.stat_710372469", // Curse Enemies with Conductivity on Hit, with #% increased Effect
            "implicit.stat_1866911844", // Socketed Gems are Supported by Level # Inspiration
            "implicit.stat_426847518", // Curse Enemies with Frostbite on Hit, with #% increased Effect
            "implicit.stat_3574189159", // Elemental Overload
            "implicit.stat_1880071428", // #% increased effect of Non-Curse Auras from your Skills
            "implicit.stat_1787073323", // Skills Chain # times
            "implicit.stat_1001077145", // Arrows Chain # times
            "implicit.stat_4223377453", // #% increased Brand Attachment range
        ];

        internal static readonly string[] lMagnitudeImplicits =
        [
            "implicit.stat_1794120699", // #% increased Prefix Modifier magnitudes
            "implicit.stat_1033086302", // #% increased Suffix Modifier magnitudes
            "implicit.stat_1581907402" // #% increased Explicit Modifier magnitudes
        ];

        internal static readonly Dictionary<string, bool> dicDefaultPosition = new()
        {
            { "stat_3441651621", true }, { "stat_3853018505", true }, { "stat_969865219", true }, { "stat_4176970656", true },
            { "stat_3277537093", true }, { "stat_3691641145", true }, { "stat_3557561376", true }, { "stat_705686721", true },
            { "stat_2156764291", true }, { "stat_3743301799", true }, { "stat_1187803783", true }, { "stat_3612407781", true },
            { "stat_496011033", true }, { "stat_1625103793", true }, { "stat_308618188", true }, { "stat_2590715472", true },
            { "stat_1964333391", true }, { "stat_614758785", true }, { "stat_2440172920", true }, { "stat_321765853", true },
            { "stat_465051235", true }, { "stat_261654754", true }, { "stat_3522931817", true }, { "stat_1443108510", true },
            { "stat_2477636501", true }//, { "stat_2901986750", true}
        };
    }
}
