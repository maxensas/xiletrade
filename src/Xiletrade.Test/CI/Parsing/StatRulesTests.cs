using FluentAssertions;
using System.Text.Json;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Test.CI.Parsing;

/// <summary>
/// The stat rules (Data/StatRules.json, Data/StatRulesTwo.json) must give the same ids as the previous code
/// (SwitchPoe1EntrieId / SwitchPoe2EntrieId, kept below as reference, which modified the entries of the filters).
/// </summary>
public class StatRulesTests
{
    private static readonly StatRuleFlag[] _poe1Flags =
        [ StatRuleFlag.Weapon, StatRuleFlag.Armour, StatRuleFlag.Unique, StatRuleFlag.Jewel, StatRuleFlag.Cluster, StatRuleFlag.Tincture,
          StatRuleFlag.Charm, StatRuleFlag.Flask, StatRuleFlag.Amulet, StatRuleFlag.Chronicle, StatRuleFlag.Logbook, StatRuleFlag.SanctumRelic ];
    private static readonly StatRuleFlag[] _poe2Flags =
        [ StatRuleFlag.Weapon, StatRuleFlag.Armour, StatRuleFlag.Unique, StatRuleFlag.Jewel, StatRuleFlag.Charm, StatRuleFlag.Flask,
          StatRuleFlag.Amulet, StatRuleFlag.Shield, StatRuleFlag.Waystone, StatRuleFlag.Tablet ];
    private static readonly string[] _entryTypes = [ "explicit", "implicit", "sanctum", "pseudo" ];

    [Theory]
    [InlineData("StatRules.json")]
    [InlineData("StatRulesTwo.json")]
    public void _01_Rule_Files_Should_Be_Valid(string fileName)
    {
        var rules = LoadRules(fileName);
        rules.Rules.Should().NotBeNullOrEmpty();
        FluentActions.Invoking(() => rules.Validate(fileName)).Should().NotThrow();
    }

    [Theory]
    [InlineData("wepon")]
    [InlineData("unique:")]
    [InlineData("name:Bones of Ullr")]
    public void _02_Validate_Should_Reject_Unknown_Conditions(string condition)
    {
        var rules = new StatRule { Rules = [ new() { When = [ condition ], Id = "explicit.stat_1" } ] };
        FluentActions.Invoking(() => rules.Validate("test.json")).Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void _03_Validate_Should_Reject_Unknown_Mode()
    {
        var rules = new StatRule { Mode = "any", Rules = [ new() { Id = "explicit.stat_1" } ] };
        FluentActions.Invoking(() => rules.Validate("test.json")).Should().Throw<InvalidDataException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void _04_Rules_Should_Give_The_Same_Ids_As_The_Previous_Code(int game)
    {
        var rules = LoadRules(game is 1 ? "StatRules.json" : "StatRulesTwo.json");
        var flags = game is 1 ? _poe1Flags : _poe2Flags;
        var random = new Random(1234);

        var ruleIds = new HashSet<string> { string.Empty, "x", "nodot", "explicit.indexable_support_999" };
        var uniqueNames = new HashSet<string>();
        CollectIdsAndUniques(rules, ruleIds, uniqueNames);
        string[] names = [ null, "Some Other Unique", .. uniqueNames ];
        string[] typeEns = [ null, "Overseer Tablet" /* Strings.Tablet.Overseer */, "Other Tablet" ];

        // every combination of up to 2 flags with every unique, then random combinations
        var items = new List<FakeItem>();
        foreach (var name in names)
        {
            items.Add(FakeItem.Create([], name, null));
            for (int i = 0; i < flags.Length; i++)
            {
                items.Add(FakeItem.Create([ flags[i] ], name, null));
                for (int j = i + 1; j < flags.Length; j++)
                {
                    items.Add(FakeItem.Create([ flags[i], flags[j] ], name, null));
                }
            }
        }
        foreach (var typeEn in typeEns)
        {
            items.Add(FakeItem.Create([ StatRuleFlag.Tablet ], null, typeEn));
            items.Add(FakeItem.Create([ StatRuleFlag.Waystone ], null, typeEn));
            items.Add(FakeItem.Create([], null, typeEn));
        }
        for (int r = 0; r < 3000; r++)
        {
            var randomFlags = flags.OrderBy(_ => random.Next()).Take(random.Next(1, 6)).ToArray();
            items.Add(FakeItem.Create(randomFlags, names[random.Next(names.Length)], typeEns[random.Next(typeEns.Length)]));
        }

        // ids of the rules (and their variants) with every item, ids of the filters with a part of the items
        foreach (var id in ruleIds)
        {
            foreach (var type in _entryTypes)
            {
                foreach (var item in items)
                {
                    Compare(game, rules, id, type, item);
                }
            }
        }
        var someItems = items.OrderBy(_ => random.Next()).Take(200).ToArray();
        foreach (var (id, type) in LoadEntries(game is 1 ? "Filters.json" : "FiltersTwo.json"))
        {
            foreach (var item in someItems)
            {
                Compare(game, rules, id, type, item);
            }
        }
    }

    private static void Compare(int game, StatRule rules, string id, string type, FakeItem item)
    {
        var entry = new OldEntry { ID = id, Type = type };
        bool oldSkip = game is 1 ? OldPoe1(entry, item) : OldPoe2(entry, item);
        var newId = rules.Resolve(id, type, item, out bool newSkip);

        // a skipped entry is not used : its id does not matter
        if (oldSkip != newSkip || !oldSkip && entry.ID != newId)
        {
            Assert.Fail($"PoE {game}, id '{id}', type '{type}', item {item} : previous code ({entry.ID}, skip {oldSkip}), rules ({newId}, skip {newSkip})");
        }
    }

    private static void CollectIdsAndUniques(StatRule rule, HashSet<string> ids, HashSet<string> uniques)
    {
        if (rule.Id is not null) ids.Add(rule.Id);
        if (rule.SuffixId is not null) ids.Add("explicit." + rule.SuffixId);
        var match = rule.Match;
        if (match is not null)
        {
            foreach (var id in match.Ids ?? []) ids.Add(id);
            foreach (var suffix in match.Suffix ?? [])
            {
                ids.Add(suffix);
                ids.Add("explicit." + suffix);
                ids.Add("implicit." + suffix);
                ids.Add("rune." + suffix);
            }
            foreach (var value in match.Contains ?? []) ids.Add("explicit." + value + "_1");
            foreach (var value in match.NotContains ?? []) ids.Add("pseudo." + value);
            foreach (var value in match.Stat ?? []) ids.Add("explicit." + value);
        }
        foreach (var condition in rule.When ?? [])
        {
            var c = condition.TrimStart('!');
            if (c.StartsWith(StatRuleExtensions.UNIQUE + ":", StringComparison.Ordinal))
            {
                uniques.Add(c[(StatRuleExtensions.UNIQUE.Length + 1)..]);
            }
        }
        foreach (var child in rule.Rules ?? [])
        {
            CollectIdsAndUniques(child, ids, uniques);
        }
    }

    private static StatRule LoadRules(string fileName)
    {
        using var stream = File.OpenRead(Path.Combine(GetDataPath(), fileName));
        return new JsonHelper().Deserialize<StatRule>(stream);
    }

    private static List<(string Id, string Type)> LoadEntries(string fileName)
    {
        using var stream = File.OpenRead(Path.Combine(GetDataPath(), "Lang", "en-US", fileName));
        using var doc = JsonDocument.Parse(stream);
        var entries = new List<(string, string)>();
        foreach (var result in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            foreach (var entry in result.GetProperty("entries").EnumerateArray())
            {
                entries.Add((entry.GetProperty("id").GetString() ?? string.Empty,
                    entry.TryGetProperty("type", out var type) ? type.GetString() ?? string.Empty : string.Empty));
            }
        }
        return entries;
    }

    private static string GetDataPath()
    {
        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = basePath[..basePath.IndexOf("bin")];
        return Path.Combine(basePath, "Data");
    }

    private sealed class OldEntry
    {
        public string ID { get; set; }
        public string Type { get; set; }
    }

    // item with the flags used by the previous code and by the rules
    private sealed class FakeItem : IStatRuleItem
    {
        public bool IsWeapon { get; init; }
        public bool IsArmour { get; init; }
        public bool Unique { get; init; }
        public bool IsJewel { get; init; }
        public bool Cluster { get; init; }
        public bool Tincture { get; init; }
        public bool Charm { get; init; }
        public bool Flask { get; init; }
        public bool Amulets { get; init; }
        public bool Shield { get; init; }
        public bool Waystones { get; init; }
        public bool Tablet { get; init; }
        public bool Chronicle { get; init; }
        public bool Logbook { get; init; }
        public bool SanctumRelic { get; init; }
        public string UniqueName { get; init; }
        public string TypeEn { get; init; }
        public string Name => UniqueName;
        private StatRuleFlag[] Flags { get; init; }

        public static FakeItem Create(StatRuleFlag[] flags, string uniqueName, string typeEn) => new()
        {
            Flags = flags,
            IsWeapon = flags.Contains(StatRuleFlag.Weapon),
            IsArmour = flags.Contains(StatRuleFlag.Armour),
            Unique = flags.Contains(StatRuleFlag.Unique),
            IsJewel = flags.Contains(StatRuleFlag.Jewel),
            Cluster = flags.Contains(StatRuleFlag.Cluster),
            Tincture = flags.Contains(StatRuleFlag.Tincture),
            Charm = flags.Contains(StatRuleFlag.Charm),
            Flask = flags.Contains(StatRuleFlag.Flask),
            Amulets = flags.Contains(StatRuleFlag.Amulet),
            Shield = flags.Contains(StatRuleFlag.Shield),
            Waystones = flags.Contains(StatRuleFlag.Waystone),
            Tablet = flags.Contains(StatRuleFlag.Tablet),
            Chronicle = flags.Contains(StatRuleFlag.Chronicle),
            Logbook = flags.Contains(StatRuleFlag.Logbook),
            SanctumRelic = flags.Contains(StatRuleFlag.SanctumRelic),
            UniqueName = uniqueName,
            TypeEn = typeEn
        };

        // previous code : words.MatchNameEn(nameEn, item.Name)
        public bool IsUniqueName(string nameEn) => UniqueName == nameEn;

        public bool HasFlag(StatRuleFlag flag) => Flags.Contains(flag);

        public bool IsUnique(ReadOnlySpan<char> nameEn) => UniqueName is not null && nameEn.SequenceEqual(UniqueName);

        public bool IsTypeEn(ReadOnlySpan<char> typeEn) => typeEn.SequenceEqual(TypeEn);

        public override string ToString() => $"[{string.Join(", ", Flags)}] unique '{UniqueName}' typeEn '{TypeEn}'";
    }

    // ---------------- previous code (reference), item flags read from FakeItem ----------------

    // Strings.StatPoe1.Aura.lSkipMods
    private static readonly string[] _skipMods =
    [
        "stat_1920370417",
        "stat_1803598623",
        "stat_2721871046",
        "stat_3484910620",
        "stat_2963485753",
        "stat_4216444167",
        "stat_3266567165",
        "stat_1761642973",
        "stat_1692887998",
        "stat_3059700363",
        "stat_3819451758",
        "stat_1542765265",
        "stat_1284151528",
        "stat_3959101898",
        "stat_2605040931",
        "stat_1135152940",
        "stat_1450978702",
        "stat_2665518524",
        "delve_corrupted_implicit"
    ];

    // Strings.StatPoe1.Temple.RoomList
    private static readonly string[] _roomList =
    [
        "pseudo_temple_apex",
        "pseudo_temple_breeding_room_3",
        "pseudo_temple_workshop_3",
        "pseudo_temple_explosives_room_3",
        "pseudo_temple_breach_room_3",
        "pseudo_temple_currency_vault_3",
        "pseudo_temple_weapon_room_3",
        "pseudo_temple_armour_room_3",
        "pseudo_temple_trinket_room_3",
        "pseudo_temple_cartography_room_3",
        "pseudo_temple_gem_room_3",
        "pseudo_temple_torment_3",
        "pseudo_temple_strongbox_3",
        "pseudo_temple_legion_3",
        "pseudo_temple_sacrifice_room_3",
        "pseudo_temple_chests_3",
        "pseudo_temple_corruption_room_3",
        "pseudo_temple_empowering_room_3",
        "pseudo_temple_storm_room_3",
        "pseudo_temple_poison_room_3",
        "pseudo_temple_trap_room_3",
        "pseudo_temple_healing_room_3",
        "pseudo_temple_boss_fire_3",
        "pseudo_temple_boss_lightning_3",
        "pseudo_temple_boss_minions_3",
        "pseudo_temple_queens_chambers_3"
    ];

    // same as lSkipMods.Contains(id.Split('.')[1]), without allocation (id without '.' : not skipped)
    private static bool IsSkipMod(ReadOnlySpan<char> id)
    {
        int dot = id.IndexOf('.');
        if (dot < 0)
        {
            return false;
        }
        var stat = id[(dot + 1)..];
        int next = stat.IndexOf('.');
        if (next >= 0)
        {
            stat = stat[..next];
        }
        foreach (var skip in _skipMods)
        {
            if (stat.SequenceEqual(skip))
            {
                return true;
            }
        }
        return false;
    }

    private static bool OldPoe1(OldEntry entrie, FakeItem item)
    {
        bool continueLoop = false;

        if (entrie.ID.Length > 1)
        {
            if (IsSkipMod(entrie.ID))
            {
                return true;
            }
            
            if (entrie.ID.Contain("indexable_support" /* Strings.Word.IndexableSupport */))
            {
                bool isShako = item.IsUniqueName("Forbidden Shako" /* Strings.Unique.ForbiddenShako */);
                bool isLioneye = item.IsUniqueName("Lioneye's Vision" /* Strings.Unique.LioneyesVision */);
                bool isBitter = item.IsUniqueName("Bitterdream" /* Strings.Unique.Bitterdream */);

                if (!isShako && !isLioneye)
                {
                    continueLoop = true;
                }
                if (entrie.ID is "explicit.indexable_support_33" /* Strings.StatPoe1.SocketedPierce2 */ && isLioneye)
                {
                    entrie.ID = "explicit.stat_254728692" /* Strings.StatPoe1.SocketedPierce1 */;
                }

                if (entrie.ID is "explicit.indexable_support_24" /* Strings.StatPoe1.SocketedInspiration2 */ && isBitter)
                {
                    entrie.ID = "explicit.stat_749770518" /* Strings.StatPoe1.SocketedInspiration1 */;
                }
            }

            // TODO : REDO duplicate mod handling
            if (entrie.ID is "explicit.stat_803737631" /* Strings.StatPoe1.Accuracy */ || entrie.ID is "explicit.stat_691932474" /* Strings.StatPoe1.AccuracyLocal */)
            {
                entrie.ID = item.IsWeapon ? "explicit.stat_691932474" /* Strings.StatPoe1.AccuracyLocal */ : "explicit.stat_803737631" /* Strings.StatPoe1.Accuracy */;
            }
            else if (entrie.ID is "explicit.stat_809229260" /* Strings.StatPoe1.Armor */ || entrie.ID is "explicit.stat_3484657501" /* Strings.StatPoe1.ArmorLocal */)
            {
                entrie.ID = item.IsArmour ? "explicit.stat_3484657501" /* Strings.StatPoe1.ArmorLocal */ : "explicit.stat_809229260" /* Strings.StatPoe1.Armor */;
            }
            else if (entrie.ID is "explicit.stat_3489782002" /* Strings.StatPoe1.Es */ || entrie.ID is "explicit.stat_4052037485" /* Strings.StatPoe1.EsLocal */)
            {
                entrie.ID = item.IsArmour ? "explicit.stat_4052037485" /* Strings.StatPoe1.EsLocal */ : "explicit.stat_3489782002" /* Strings.StatPoe1.Es */;
            }
            else if (entrie.ID is "explicit.stat_2144192055" /* Strings.StatPoe1.Eva */ || entrie.ID is "explicit.stat_53045048" /* Strings.StatPoe1.EvaLocal */)
            {
                entrie.ID = item.IsArmour ? "explicit.stat_53045048" /* Strings.StatPoe1.EvaLocal */ : "explicit.stat_2144192055" /* Strings.StatPoe1.Eva */;
            }
            else if (entrie.ID is "explicit.stat_3503466234" /* Strings.StatPoe1.HitBlind1 */ || entrie.ID is "explicit.stat_3565956680" /* Strings.StatPoe1.HitBlind2 */)
            {
                entrie.ID = item.IsArmour ? "explicit.stat_3565956680" /* Strings.StatPoe1.HitBlind2 */ : "explicit.stat_3503466234" /* Strings.StatPoe1.HitBlind1 */;
            }
            else if (entrie.ID is "explicit.stat_2361218755" /* Strings.StatPoe1.ImmunityIgnite1 */ || entrie.ID is "explicit.stat_2695527599" /* Strings.StatPoe1.ImmunityIgnite2 */)
            {
                entrie.ID = item.Unique ? "explicit.stat_2695527599" /* Strings.StatPoe1.ImmunityIgnite2 */ : "explicit.stat_2361218755" /* Strings.StatPoe1.ImmunityIgnite1 */;
            }
            else if (entrie.ID is "explicit.stat_3382957283" /* Strings.StatPoe1.TriggerAssassinOld */)
            {
                entrie.ID = "explicit.stat_3924520095" /* Strings.StatPoe1.TriggerAssassinNew */;
            }
            else if (entrie.ID is "explicit.stat_1269219558" /* Strings.StatPoe1.IncManaReserveEffOld */)
            {
                entrie.ID = "explicit.stat_4237190083" /* Strings.StatPoe1.IncManaReserveEffNew */;
            }
            else if (entrie.ID is "explicit.stat_492027537" /* Strings.StatPoe1.SupressOld */)
            {
                entrie.ID = "explicit.stat_3680664274" /* Strings.StatPoe1.SupressNew */;
            }
            else if (entrie.ID is "explicit.stat_2858921304" /* Strings.StatPoe1.CritFlaskChargeOld */) // many rarity
            {
                entrie.ID = "explicit.stat_3738001379" /* Strings.StatPoe1.CritFlaskChargeNew */;
            }
            else if (entrie.ID is "explicit.stat_1291925008" /* Strings.StatPoe1.PrecisionEfficiencyOld */) // Hyrri's Truth
            {
                entrie.ID = "explicit.stat_3859865977" /* Strings.StatPoe1.PrecisionEfficiencyNew */;
            }
            else if (entrie.ID is "explicit.stat_2530372417" /* Strings.StatPoe1.BlockAttack1 */ || entrie.ID is "explicit.stat_1702195217" /* Strings.StatPoe1.BlockAttack2 */)
            {
                entrie.ID = item.IsJewel && item.Unique ? "explicit.stat_1702195217" /* Strings.StatPoe1.BlockAttack2 */ : "explicit.stat_2530372417" /* Strings.StatPoe1.BlockAttack1 */;
            }
            else if (entrie.ID is "explicit.stat_561307714" /* Strings.StatPoe1.BlockSpell1 */ || entrie.ID is "explicit.stat_19803471" /* Strings.StatPoe1.BlockSpell2 */)
            {
                entrie.ID = item.IsJewel && item.Unique ? "explicit.stat_19803471" /* Strings.StatPoe1.BlockSpell2 */ : "explicit.stat_561307714" /* Strings.StatPoe1.BlockSpell1 */;
            }
            else if (entrie.ID is "explicit.stat_1004011302" /* Strings.StatPoe1.CoolDownRecovery1 */ || entrie.ID is "explicit.stat_239144" /* Strings.StatPoe1.CoolDownRecovery2 */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_239144" /* Strings.StatPoe1.CoolDownRecovery2 */ : "explicit.stat_1004011302" /* Strings.StatPoe1.CoolDownRecovery1 */;
            }
            else if (entrie.ID is "explicit.stat_165218607" /* Strings.StatPoe1.IncCritAgainst1 */ && item.IsJewel && item.Unique)
            {
                entrie.ID = "explicit.stat_4270096386" /* Strings.StatPoe1.IncCritAgainst2 */;
            }
            else if (entrie.ID is "explicit.stat_2653955271" /* Strings.StatPoe1.PeneFire */ || entrie.ID is "explicit.stat_1123291426" /* Strings.StatPoe1.PeneFireTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_1123291426" /* Strings.StatPoe1.PeneFireTincture */ : "explicit.stat_2653955271" /* Strings.StatPoe1.PeneFire */;
            }
            else if (entrie.ID is "explicit.stat_3417711605" /* Strings.StatPoe1.PeneCold */ || entrie.ID is "explicit.stat_1211769158" /* Strings.StatPoe1.PeneColdTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_1211769158" /* Strings.StatPoe1.PeneColdTincture */ : "explicit.stat_3417711605" /* Strings.StatPoe1.PeneCold */;
            }
            else if (entrie.ID is "explicit.stat_818778753" /* Strings.StatPoe1.PeneLight */ || entrie.ID is "explicit.stat_3301510262" /* Strings.StatPoe1.PeneLightTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_3301510262" /* Strings.StatPoe1.PeneLightTincture */ : "explicit.stat_818778753" /* Strings.StatPoe1.PeneLight */;
            }
            else if (entrie.ID is "explicit.stat_1368271171" /* Strings.StatPoe1.ManaPerKill */ || entrie.ID is "explicit.stat_782259898" /* Strings.StatPoe1.ManaPerKillTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_782259898" /* Strings.StatPoe1.ManaPerKillTincture */ : "explicit.stat_1368271171" /* Strings.StatPoe1.ManaPerKill */;
            }
            else if (entrie.ID is "explicit.stat_3481736410" /* Strings.StatPoe1.AoeKill */ || entrie.ID is "explicit.stat_923608573" /* Strings.StatPoe1.AoeKillTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_923608573" /* Strings.StatPoe1.AoeKillTincture */ : "explicit.stat_3481736410" /* Strings.StatPoe1.AoeKill */;
            }
            else if (entrie.ID is "explicit.stat_2355615476" /* Strings.StatPoe1.CritFullLife */ || entrie.ID is "explicit.stat_3735443206" /* Strings.StatPoe1.CritFullLifeTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_3735443206" /* Strings.StatPoe1.CritFullLifeTincture */ : "explicit.stat_2355615476" /* Strings.StatPoe1.CritFullLife */;
            }
            else if (entrie.ID is "explicit.stat_2918708827" /* Strings.StatPoe1.PhasingKill */ || entrie.ID is "explicit.stat_3669845133" /* Strings.StatPoe1.PhasingKillTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_3669845133" /* Strings.StatPoe1.PhasingKillTincture */ : "explicit.stat_2918708827" /* Strings.StatPoe1.PhasingKill */;
            }
            else if (entrie.ID is "explicit.stat_3135669941" /* Strings.StatPoe1.ConcGround */ || entrie.ID is "explicit.stat_4278270018" /* Strings.StatPoe1.ConcGroundTincture */)
            {
                entrie.ID = item.Tincture ? "explicit.stat_4278270018" /* Strings.StatPoe1.ConcGroundTincture */ : "explicit.stat_3135669941" /* Strings.StatPoe1.ConcGround */;
            }
            else if (entrie.ID is "explicit.stat_2264295449" /* Strings.StatPoe1.StrikeRange */ && item.Tincture)
            {
                entrie.ID = "explicit.stat_3369332977" /* Strings.StatPoe1.StrikeRangeTincture */;
            }
            else if (entrie.ID is "explicit.stat_1535626285" /* Strings.StatPoe1.StrInt */ && item.Charm)
            {
                entrie.ID = "explicit.stat_2543977012" /* Strings.StatPoe1.StrIntCharm */;
            }
            else if (entrie.ID is "explicit.stat_2530372417" /* Strings.StatPoe1.BlockDmg */ || entrie.ID is "explicit.stat_1702195217" /* Strings.StatPoe1.BlockDmgJewCharm */)
            {
                entrie.ID = item.Charm || item.IsJewel ? "explicit.stat_1702195217" /* Strings.StatPoe1.BlockDmgJewCharm */ : "explicit.stat_2530372417" /* Strings.StatPoe1.BlockDmg */;
            }
            else if (entrie.ID is "explicit.stat_3023957681" /* Strings.StatPoe1.Onslaught */
                || entrie.ID is "explicit.stat_665823128" /* Strings.StatPoe1.OnslaughtWeaponCharm */
                || entrie.ID is "explicit.stat_2453026567" /* Strings.StatPoe1.OnslaughtAmulet */)
            {
                entrie.ID = item.Charm || item.IsWeapon ? "explicit.stat_665823128" /* Strings.StatPoe1.OnslaughtWeaponCharm */
                    : item.Amulets && item.Unique ? "explicit.stat_2453026567" /* Strings.StatPoe1.OnslaughtAmulet */ : "explicit.stat_3023957681" /* Strings.StatPoe1.Onslaught */;
            }
            else if (entrie.ID is "explicit.stat_3859593448" /* Strings.StatPoe1.ReduceEle */ || entrie.ID is "explicit.stat_983989924" /* Strings.StatPoe1.ReduceEleGorgon */)
            {
                bool isGorgon = item.IsUniqueName("Gorgon's Gaze" /* Strings.Unique.GorgonsGaze */);
                entrie.ID = isGorgon ? "explicit.stat_983989924" /* Strings.StatPoe1.ReduceEleGorgon */ : "explicit.stat_3859593448" /* Strings.StatPoe1.ReduceEle */;
            }
            else if (entrie.ID is "explicit.stat_424549222" /* Strings.StatPoe1.ShockSpread */ || entrie.ID is "explicit.stat_1640259660" /* Strings.StatPoe1.ShockSpreadEsh */)
            {
                bool isEsh = item.IsUniqueName("Esh's Mirror" /* Strings.Unique.EshsMirror */);
                entrie.ID = isEsh ? "explicit.stat_1640259660" /* Strings.StatPoe1.ShockSpreadEsh */ : "explicit.stat_424549222" /* Strings.StatPoe1.ShockSpread */;
            }
            else if (entrie.ID is "explicit.indexable_skill_16" /* Strings.StatPoe1.Zombie */ || entrie.ID is "explicit.stat_2739830820" /* Strings.StatPoe1.ZombieBones */)
            {
                bool isUllr = item.IsUniqueName("Bones of Ullr" /* Strings.Unique.BonesOfUllr */);
                entrie.ID = isUllr ? "explicit.stat_2739830820" /* Strings.StatPoe1.ZombieBones */ : "explicit.indexable_skill_16" /* Strings.StatPoe1.Zombie */;
            }
            else if (entrie.ID is "explicit.indexable_skill_29" /* Strings.StatPoe1.Spectre */ || entrie.ID is "explicit.stat_3235814433" /* Strings.StatPoe1.SpectreBones */)
            {
                bool isUllr = item.IsUniqueName("Bones of Ullr" /* Strings.Unique.BonesOfUllr */);
                entrie.ID = isUllr ? "explicit.stat_3235814433" /* Strings.StatPoe1.SpectreBones */ : "explicit.indexable_skill_29" /* Strings.StatPoe1.Spectre */;
            }
            else if (item.Flask && item.Unique)
            {
                bool isCinder = item.IsUniqueName("Cinderswallow Urn" /* Strings.Unique.CinderswallowUrn */);
                bool isDiv = item.IsUniqueName("Divination Distillate" /* Strings.Unique.DivinationDistillate */);

                entrie.ID = entrie.ID is "explicit.stat_1740200922" /* Strings.StatPoe1.FlaskIncRarity1 */ && isCinder ? "explicit.stat_3251705960" /* Strings.StatPoe1.FlaskIncRarity2 */
                    : entrie.ID is "explicit.stat_3251705960" /* Strings.StatPoe1.FlaskIncRarity2 */ && isDiv ? "explicit.stat_1740200922" /* Strings.StatPoe1.FlaskIncRarity1 */
                    : entrie.ID;
            }
            else if (item.IsJewel && item.Unique)
            {
                if (entrie.ID is "explicit.stat_1224928411" /* Strings.StatPoe1.TheBlueNightmare */)
                {
                    bool isBlueDream = item.IsUniqueName("The Blue Dream" /* Strings.Unique.TheBlueDream */);
                    if (isBlueDream)
                    {
                        entrie.ID = "explicit.stat_926444104" /* Strings.StatPoe1.TheBlueDream */;
                    }
                }
            }
            else if (item.IsArmour && item.Unique)
            {
                if (entrie.ID is "explicit.stat_1029319062" /* Strings.StatPoe1.FireTakenOld */) // The Rat Cage
                {
                    entrie.ID = "explicit.stat_3205239847" /* Strings.StatPoe1.FireTakenNew */;
                }
                if (entrie.ID is "explicit.stat_151975117" /* Strings.StatPoe1.PurityIce1 */) //  Doryani's Delusion
                {
                    entrie.ID = "explicit.stat_4193390599" /* Strings.StatPoe1.PurityIce2 */;
                }
                if (entrie.ID is "explicit.stat_3716281760" /* Strings.StatPoe1.PurityFire1 */) //  Doryani's Delusion
                {
                    entrie.ID = "explicit.stat_3970432307" /* Strings.StatPoe1.PurityFire2 */;
                }
                if (entrie.ID is "explicit.stat_1141249906" /* Strings.StatPoe1.PurityLightning1 */) //  Doryani's Delusion
                {
                    entrie.ID = "explicit.stat_3822878124" /* Strings.StatPoe1.PurityLightning2 */;
                }
            }
            else if (item.Chronicle)
            {
                bool goContinue = true;
                for (int s = 0; s < _roomList.Length; s++)
                {
                    if (entrie.ID.Contain(_roomList[s]))
                    {
                        goContinue = false;
                        break;
                    }
                }
                if (goContinue) continueLoop = true;
            }
            else if (item.IsWeapon && item.Unique)
            {
                if (entrie.ID is "explicit.stat_2523146878" /* Strings.StatPoe1.PoisonMoreDmg1 */) // Darkscorn old mod
                {
                    entrie.ID = "explicit.stat_768124628" /* Strings.StatPoe1.PoisonMoreDmg2 */;
                }
                bool isDervish = item.IsUniqueName("The Dancing Dervish" /* Strings.Unique.TheDancingDervish */);
                if (entrie.ID is "explicit.stat_2397408229" /* Strings.StatPoe1.Rampage */ && isDervish)
                {
                    continueLoop = true;
                }
                bool isTrypanon = item.IsUniqueName("Replica Trypanon" /* Strings.Unique.ReplicaTrypanon */);
                if (entrie.ID is "explicit.stat_691932474" /* Strings.StatPoe1.AccuracyLocal */ && isTrypanon) // this is not a revert from previous code lines
                {
                    entrie.ID = "explicit.stat_803737631" /* Strings.StatPoe1.Accuracy */;
                }
                bool isNetolKiss = item.IsUniqueName("Uul-Netol's Kiss" /* Strings.Unique.UulNetolsKiss */);
                if (entrie.ID is "explicit.stat_3967845372" /* Strings.StatPoe1.CurseVulnerability */ && isNetolKiss)
                {
                    entrie.ID = "explicit.stat_2213584313" /* Strings.StatPoe1.CurseVulnerabilityChance */;
                }
            }
            else if (item.SanctumRelic)
            {
                if (entrie.Type is not "sanctum" /* Strings.Word.Sanctum */)
                {
                    continueLoop = true;
                }
            }
            else if (item.Cluster)
            {
                if (entrie.ID is "enchant.stat_3948993189|25" /* Strings.StatPoe1.ClusterCurseEffect1 */)
                {
                    entrie.ID = "enchant.stat_3948993189|55" /* Strings.StatPoe1.ClusterCurseEffect2 */;
                }
            }
        }

        if (item.Logbook)//&& implicitMod
        {
            if (!entrie.ID.Contain("stat_3159649981" /* Strings.StatPoe1.Generic.LogbookBoss */)
                && !entrie.ID.Contain("stat_1160596338" /* Strings.StatPoe1.Generic.LogbookArea */)
                && !entrie.ID.Contain("stat_3239978999" /* Strings.StatPoe1.Generic.LogbookTwice */))
            {
                continueLoop = true;
            }
        }

        return continueLoop;
    }

    private static bool OldPoe2(OldEntry entrie, FakeItem item)
    {
        bool continueLoop = false;

        if (entrie.ID.Length is 0)
        {
            return false;
        }

        if (item.Waystones || item.Tablet)
        {
            if (entrie.ID is "explicit.stat_3666934677" /* Strings.StatPoe2.IncXpGain1 */)
            {
                entrie.ID = "explicit.stat_57434274" /* Strings.StatPoe2.IncXpGain2 */;
            }
            if (entrie.ID is "explicit.stat_1174954559" /* Strings.StatPoe2.DeliFog1 */)
            {
                entrie.ID = "explicit.stat_3226351972" /* Strings.StatPoe2.DeliFog2 */;
            }
        }
        else
        {
            if (entrie.ID is "explicit.stat_57434274" /* Strings.StatPoe2.IncXpGain2 */)
            {
                entrie.ID = "explicit.stat_3666934677" /* Strings.StatPoe2.IncXpGain1 */;
            }
            if (entrie.ID is "explicit.stat_3226351972" /* Strings.StatPoe2.DeliFog2 */)
            {
                entrie.ID = "explicit.stat_1174954559" /* Strings.StatPoe2.DeliFog1 */;
            }
        }

        if (item.Flask)
        {
            if (entrie.ID is "explicit.stat_2541588185" /* Strings.StatPoe2.IncDuration2 */)
            {
                entrie.ID = "explicit.stat_1256719186" /* Strings.StatPoe2.IncDuration1 */;
            }
        }
        if (item.Charm)
        {
            if (entrie.ID is "explicit.stat_1256719186" /* Strings.StatPoe2.IncDuration1 */)
            {
                entrie.ID = "explicit.stat_2541588185" /* Strings.StatPoe2.IncDuration2 */;
            }
        }

        if (item.Unique && item.Amulets)
        {
            if (entrie.ID is "skill.lightning_bolt" /* Strings.StatPoe2.SkillLightningBolt */)
            {
                entrie.ID = "skill.unique_breach_lightning_bolt" /* Strings.StatPoe2.SkillLightningBoltUnique */;
            }
        }
        else
        {
            if (entrie.ID is "skill.unique_breach_lightning_bolt" /* Strings.StatPoe2.SkillLightningBoltUnique */)
            {
                entrie.ID = "skill.lightning_bolt" /* Strings.StatPoe2.SkillLightningBolt */;
            }
        }

        if (item.IsJewel)
        {
            if (entrie.ID is "explicit.stat_1030153674" /* Strings.StatPoe2.RecoverManaKill1 */)
            {
                entrie.ID = "explicit.stat_1604736568" /* Strings.StatPoe2.RecoverManaKill2 */;
            }
        }
        else
        {
            if (entrie.ID is "explicit.stat_1604736568" /* Strings.StatPoe2.RecoverManaKill2 */)
            {
                entrie.ID = "explicit.stat_1030153674" /* Strings.StatPoe2.RecoverManaKill1 */;
            }
        }

        if (item.Shield)
        {
            if (entrie.ID is "explicit.stat_4147897060" /* Strings.StatPoe2.IncBlock2 */)
            {
                entrie.ID = "explicit.stat_2481353198" /* Strings.StatPoe2.IncBlock1 */;
            }
        }
        else
        {
            if (entrie.ID is "explicit.stat_2481353198" /* Strings.StatPoe2.IncBlock1 */)
            {
                entrie.ID = "explicit.stat_4147897060" /* Strings.StatPoe2.IncBlock2 */;
            }
        }

        if (item.IsWeapon)
        {
            if (entrie.ID is "explicit.stat_681332047" /* Strings.StatPoe2.IncAs2 */)
            {
                entrie.ID = "explicit.stat_210067635" /* Strings.StatPoe2.IncAs1 */;
            }
            if (entrie.ID is "explicit.stat_803737631" /* Strings.StatPoe2.AccuracyRating2 */)
            {
                entrie.ID = "explicit.stat_691932474" /* Strings.StatPoe2.AccuracyRating1 */;
            }
            if (entrie.ID is "explicit.stat_795138349" /* Strings.StatPoe2.ChancePoison2 */)
            {
                entrie.ID = "explicit.stat_3885634897" /* Strings.StatPoe2.ChancePoison1 */;
            }
            if (entrie.ID is "explicit.stat_720908147" /* Strings.StatPoe2.AsPerDex1 */ or "explicit.stat_2241560081" /* Strings.StatPoe2.AsPerDex3 */)
            {
                entrie.ID = "explicit.stat_889691035" /* Strings.StatPoe2.AsPerDex2 */;
            }
            if (entrie.ID.EndWith("stat_1379411836" /* Strings.StatPoe2.AllAttributes1 */))
            {
                var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
                entrie.ID = kind.ToString() + "stat_2897413282" /* Strings.StatPoe2.AllAttributes2 */;
            }
        }
        else
        {
            if (entrie.ID is "explicit.stat_210067635" /* Strings.StatPoe2.IncAs1 */)
            {
                entrie.ID = "explicit.stat_681332047" /* Strings.StatPoe2.IncAs2 */;
            }
            if (entrie.ID is "explicit.stat_691932474" /* Strings.StatPoe2.AccuracyRating1 */)
            {
                entrie.ID = "explicit.stat_803737631" /* Strings.StatPoe2.AccuracyRating2 */;
            }
            if (entrie.ID is "explicit.stat_3885634897" /* Strings.StatPoe2.ChancePoison1 */)
            {
                entrie.ID = "explicit.stat_795138349" /* Strings.StatPoe2.ChancePoison2 */;
            }
            if (entrie.ID is "explicit.stat_889691035" /* Strings.StatPoe2.AsPerDex2 */ or "explicit.stat_2241560081" /* Strings.StatPoe2.AsPerDex3 */)
            {
                entrie.ID = "explicit.stat_720908147" /* Strings.StatPoe2.AsPerDex1 */;
            }
            if (entrie.ID.EndWith("stat_2897413282" /* Strings.StatPoe2.AllAttributes2 */))
            {
                var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
                entrie.ID = kind.ToString() + "stat_1379411836" /* Strings.StatPoe2.AllAttributes1 */;
            }
        }

        if (item.IsArmour)
        {
            if (entrie.ID is "explicit.stat_2866361420" /* Strings.StatPoe2.IncArmour2 */)
            {
                entrie.ID = "explicit.stat_1062208444" /* Strings.StatPoe2.IncArmour1 */;
            }
            if (entrie.ID is "explicit.stat_2106365538" /* Strings.StatPoe2.IncEvasion2 */)
            {
                entrie.ID = "explicit.stat_124859000" /* Strings.StatPoe2.IncEvasion1 */;
            }
            if (entrie.ID is "explicit.stat_2144192055" /* Strings.StatPoe2.EvasionRating1 */)
            {
                entrie.ID = "explicit.stat_53045048" /* Strings.StatPoe2.EvasionRating2 */;
            }
            if (entrie.ID is "explicit.stat_809229260" /* Strings.StatPoe2.Armour2 */)
            {
                entrie.ID = "explicit.stat_3484657501" /* Strings.StatPoe2.Armour1 */;
            }
            if (entrie.ID is "explicit.stat_3489782002" /* Strings.StatPoe2.EnergyShield1 */)
            {
                entrie.ID = "explicit.stat_4052037485" /* Strings.StatPoe2.EnergyShield2 */;
            }
            if (entrie.ID is "enchant.stat_2866361420" /* Strings.StatPoe2.IncArmourEnch2 */)
            {
                entrie.ID = "enchant.stat_1062208444" /* Strings.StatPoe2.IncArmourEnch1 */;
            }
            if (entrie.ID is "enchant.stat_2106365538" /* Strings.StatPoe2.IncEvasionEnch2 */)
            {
                entrie.ID = "enchant.stat_124859000" /* Strings.StatPoe2.IncEvasionEnch1 */;
            }
            if (entrie.ID is "explicit.stat_1416292992" /* Strings.StatPoe2.CharmSlot1 */)
            {
                entrie.ID = "explicit.stat_554899692" /* Strings.StatPoe2.CharmSlot2 */;
            }
        }
        else
        {
            if (entrie.ID is "explicit.stat_1062208444" /* Strings.StatPoe2.IncArmour1 */)
            {
                entrie.ID = "explicit.stat_2866361420" /* Strings.StatPoe2.IncArmour2 */;
            }
            if (entrie.ID is "explicit.stat_124859000" /* Strings.StatPoe2.IncEvasion1 */)
            {
                entrie.ID = "explicit.stat_2106365538" /* Strings.StatPoe2.IncEvasion2 */;
            }
            if (entrie.ID is "explicit.stat_53045048" /* Strings.StatPoe2.EvasionRating2 */)
            {
                entrie.ID = "explicit.stat_2144192055" /* Strings.StatPoe2.EvasionRating1 */;
            }
            if (entrie.ID is "explicit.stat_3484657501" /* Strings.StatPoe2.Armour1 */)
            {
                entrie.ID = "explicit.stat_809229260" /* Strings.StatPoe2.Armour2 */;
            }
            if (entrie.ID is "explicit.stat_4052037485" /* Strings.StatPoe2.EnergyShield2 */)
            {
                entrie.ID = "explicit.stat_3489782002" /* Strings.StatPoe2.EnergyShield1 */;
            }
            if (entrie.ID is "enchant.stat_2866361420" /* Strings.StatPoe2.IncArmourEnch2 */)
            {
                entrie.ID = "enchant.stat_1062208444" /* Strings.StatPoe2.IncArmourEnch1 */;
            }
            if (entrie.ID is "enchant.stat_124859000" /* Strings.StatPoe2.IncEvasionEnch1 */)
            {
                entrie.ID = "enchant.stat_2106365538" /* Strings.StatPoe2.IncEvasionEnch2 */;
            }
            if (entrie.ID is "explicit.stat_554899692" /* Strings.StatPoe2.CharmSlot2 */)
            {
                entrie.ID = "explicit.stat_1416292992" /* Strings.StatPoe2.CharmSlot1 */;
            }
        }

        //tablets
        if (entrie.ID is "explicit.stat_3042527515" /* Strings.StatPoe2.Shrine1 */ or "explicit.stat_1468737867" /* Strings.StatPoe2.Shrine2 */)
        {
            bool isOverseer = item.TypeEn is "Overseer Tablet" /* Strings.Tablet.Overseer */;
            entrie.ID = isOverseer ? "explicit.stat_3042527515" /* Strings.StatPoe2.Shrine1 */ : "explicit.stat_1468737867" /* Strings.StatPoe2.Shrine2 */;
        }
        if (entrie.ID is "explicit.stat_2162684861" /* Strings.StatPoe2.Essence1 */ or "explicit.stat_395808938" /* Strings.StatPoe2.Essence2 */)
        {
            bool isOverseer = item.TypeEn is "Overseer Tablet" /* Strings.Tablet.Overseer */;
            entrie.ID = isOverseer ? "explicit.stat_2162684861" /* Strings.StatPoe2.Essence1 */ : "explicit.stat_395808938" /* Strings.StatPoe2.Essence2 */;
        }

        //uniques
        
        if (entrie.ID.EndWith("stat_2704225257" /* Strings.StatPoe2.Spirit1 */) || entrie.ID.EndWith("stat_3981240776" /* Strings.StatPoe2.Spirit2 */))
        {
            bool isUnborn = item.IsUniqueName("The Unborn Lich" /* Strings.UniqueTwo.TheUnbornLich */);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isUnborn ? "stat_2704225257" /* Strings.StatPoe2.Spirit1 */ : "stat_3981240776" /* Strings.StatPoe2.Spirit2 */);
        }
        if (entrie.ID.EndWith("stat_1416406066" /* Strings.StatPoe2.IncSpirit1 */) || entrie.ID.EndWith("stat_3984865854" /* Strings.StatPoe2.IncSpirit2 */))
        {
            bool isUnique = item.IsUniqueName("Grip of Kulemak" /* Strings.UniqueTwo.GripofKulemak */)
                || item.IsUniqueName("Idol of Uldurn" /* Strings.UniqueTwo.IdolofUldurn */);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isUnique ? "stat_1416406066" /* Strings.StatPoe2.IncSpirit1 */ : "stat_3984865854" /* Strings.StatPoe2.IncSpirit2 */);
        }
        if (entrie.ID.EndWith("stat_774059442" /* Strings.StatPoe2.RunicWard1 */) || entrie.ID.EndWith("stat_3336230913" /* Strings.StatPoe2.RunicWard2 */))
        {
            bool isSvalinn = item.IsUniqueName("Svalinn" /* Strings.UniqueTwo.Svalinn */);
            var kind = entrie.ID.AsSpan().FirstPartIncluding('.');
            entrie.ID = kind.ToString() + (isSvalinn ? "stat_774059442" /* Strings.StatPoe2.RunicWard1 */ : "stat_3336230913" /* Strings.StatPoe2.RunicWard2 */);
        }
        if (entrie.ID is "explicit.stat_1315418254" /* Strings.StatPoe2.Zealot1 */ or "explicit.stat_3831171903|33" /* Strings.StatPoe2.Zealot2 */)
        {
            bool isGeoFri = item.IsUniqueName("Geofri's Sanctuary" /* Strings.UniqueTwo.Geofri */);
            entrie.ID = isGeoFri ? "explicit.stat_1315418254" /* Strings.StatPoe2.Zealot1 */ : "explicit.stat_3831171903|33" /* Strings.StatPoe2.Zealot2 */;
        }
        if (entrie.ID is "explicit.stat_2080373320" /* Strings.StatPoe2.Blinded1 */ or "explicit.stat_1464727508" /* Strings.StatPoe2.Blinded2 */)
        {
            bool isVestige = item.IsUniqueName("Vestige of Darkness" /* Strings.UniqueTwo.Vestige */);
            entrie.ID = isVestige ? "explicit.stat_2080373320" /* Strings.StatPoe2.Blinded1 */ : "explicit.stat_1464727508" /* Strings.StatPoe2.Blinded2 */;
        }
        // the text on one line (explicit.stat_3917489142) is a mod of its own : Loreweave can have it (item level 65)
        if (item.Unique && entrie.ID is "explicit.stat_2261942307" /* Strings.StatPoe2.Rarity1 */
            or "explicit.stat_1602191394" /* Strings.StatPoe2.Rarity2 */)
        {
            bool isLoreweave = item.IsUniqueName("Loreweave" /* Strings.UniqueTwo.Loreweave */);
            bool isGravebind = item.IsUniqueName("Gravebind" /* Strings.UniqueTwo.Gravebind */);
            entrie.ID = isLoreweave ? "explicit.stat_2261942307" /* Strings.StatPoe2.Rarity1 */ : isGravebind ? "explicit.stat_1602191394" /* Strings.StatPoe2.Rarity2 */ : entrie.ID;
        }
        if (entrie.ID is "explicit.stat_2257118425" /* Strings.StatPoe2.VaalPact1 */ or "explicit.stat_3831171903|20" /* Strings.StatPoe2.VaalPact2 */)
        {
            bool isAcuity = item.IsUniqueName("Atziri's Acuity" /* Strings.UniqueTwo.Acuity */);
            entrie.ID = isAcuity ? "explicit.stat_2257118425" /* Strings.StatPoe2.VaalPact1 */ : "explicit.stat_3831171903|20" /* Strings.StatPoe2.VaalPact2 */;
        }
        if (entrie.ID is "explicit.stat_2933846633" /* Strings.StatPoe2.Daze1 */ or "explicit.stat_3146310524" /* Strings.StatPoe2.Daze2 */)
        {
            bool isNazir = item.IsUniqueName("Nazir's Judgement" /* Strings.UniqueTwo.NazirsJudgement */);
            entrie.ID = isNazir ? "explicit.stat_2933846633" /* Strings.StatPoe2.Daze1 */ : "explicit.stat_3146310524" /* Strings.StatPoe2.Daze2 */;
        }
        if (entrie.ID is "explicit.stat_1157523820" /* Strings.StatPoe2.Aftershocks1 */ or "explicit.stat_2045949233" /* Strings.StatPoe2.Aftershocks2 */)
        {
            bool isHrimnors = item.IsUniqueName("Hrimnor's Hymn" /* Strings.UniqueTwo.HrimnorsHymn */);
            entrie.ID = isHrimnors ? "explicit.stat_2045949233" /* Strings.StatPoe2.Aftershocks2 */ : "explicit.stat_1157523820" /* Strings.StatPoe2.Aftershocks1 */;
        }
        if (entrie.ID is "explicit.stat_2625554454" /* Strings.StatPoe2.RandomShrine1 */ or "explicit.stat_2879778895" /* Strings.StatPoe2.RandomShrine2 */)
        {
            bool isHammer = item.IsUniqueName("The Hammer of Faith" /* Strings.UniqueTwo.TheHammerofFaith */);
            entrie.ID = isHammer ? "explicit.stat_2879778895" /* Strings.StatPoe2.RandomShrine2 */ : "explicit.stat_2625554454" /* Strings.StatPoe2.RandomShrine1 */;
        }
        if (entrie.ID is "explicit.stat_2582079000" /* Strings.StatPoe2.CharmSlot3 */ or "explicit.stat_554899692" /* Strings.StatPoe2.CharmSlot2 */)
        {
            bool isElevore = item.IsUniqueName("Elevore" /* Strings.UniqueTwo.Elevore */);
            entrie.ID = isElevore ? "explicit.stat_554899692" /* Strings.StatPoe2.CharmSlot2 */ : "explicit.stat_2582079000" /* Strings.StatPoe2.CharmSlot3 */;
        }
        if (entrie.ID is "skill.corpse_cloud_triggered" /* Strings.StatPoe2.Decompose1 */ or "skill.corpse_cloud" /* Strings.StatPoe2.Decompose2 */)
        {
            bool isCorpsewade = item.IsUniqueName("Corpsewade" /* Strings.UniqueTwo.Corpsewade */);
            entrie.ID = isCorpsewade ? "skill.corpse_cloud_triggered" /* Strings.StatPoe2.Decompose1 */ : "skill.corpse_cloud" /* Strings.StatPoe2.Decompose2 */;
        }
        if (entrie.ID is "explicit.stat_448592698|160" /* Strings.StatPoe2.SkeletalSniper1 */)
        {
            entrie.ID = "explicit.stat_448592698|161" /* Strings.StatPoe2.SkeletalSniper2 */;
        }
        if (entrie.ID is "explicit.stat_448592698|186" /* Strings.StatPoe2.HeraldofBlood1 */)
        {
            entrie.ID = "explicit.stat_448592698|199" /* Strings.StatPoe2.HeraldofBlood2 */;
        }
        if (entrie.ID is "explicit.stat_448592698|193" /* Strings.StatPoe2.TamedCompanion1 */)
        {
            entrie.ID = "explicit.stat_448592698|201" /* Strings.StatPoe2.TamedCompanion2 */;
        }
        bool IsFlesh(ReadOnlySpan<char> name)
            => item.IsUniqueName("Flesh Crucible" /* Strings.UniqueTwo.FleshCrucible */);

        if (entrie.ID is "explicit.stat_3831171903|7" /* Strings.StatPoe2.PainAttunement1 */ or "explicit.stat_98977150" /* Strings.StatPoe2.PainAttunement2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|7" /* Strings.StatPoe2.PainAttunement1 */ : "explicit.stat_98977150" /* Strings.StatPoe2.PainAttunement2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|2" /* Strings.StatPoe2.GiantsBlood1 */ or "explicit.stat_1875158664" /* Strings.StatPoe2.GiantsBlood2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|2" /* Strings.StatPoe2.GiantsBlood1 */ : "explicit.stat_1875158664" /* Strings.StatPoe2.GiantsBlood2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|3" /* Strings.StatPoe2.UnwaveringStance1 */ or "explicit.stat_1683578560" /* Strings.StatPoe2.UnwaveringStance2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|3" /* Strings.StatPoe2.UnwaveringStance1 */ : "explicit.stat_1683578560" /* Strings.StatPoe2.UnwaveringStance2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|9" /* Strings.StatPoe2.EldritchBattery1 */ or "explicit.stat_2262736444" /* Strings.StatPoe2.EldritchBattery2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|9" /* Strings.StatPoe2.EldritchBattery1 */ : "explicit.stat_2262736444" /* Strings.StatPoe2.EldritchBattery2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|5" /* Strings.StatPoe2.BloodMagic1 */ or "explicit.stat_2801937280" /* Strings.StatPoe2.BloodMagic2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|5" /* Strings.StatPoe2.BloodMagic1 */ : "explicit.stat_2801937280" /* Strings.StatPoe2.BloodMagic2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|21" /* Strings.StatPoe2.IronReflexes1 */ or "explicit.stat_326965591" /* Strings.StatPoe2.IronReflexes2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|21" /* Strings.StatPoe2.IronReflexes1 */ : "explicit.stat_326965591" /* Strings.StatPoe2.IronReflexes2 */;
        }
        if (entrie.ID is "explicit.stat_3831171903|22" /* Strings.StatPoe2.GlancingBlows1 */ or "explicit.stat_4266776872" /* Strings.StatPoe2.GlancingBlows2 */)
        {
            entrie.ID = IsFlesh(item.Name) ? "explicit.stat_3831171903|22" /* Strings.StatPoe2.GlancingBlows1 */ : "explicit.stat_4266776872" /* Strings.StatPoe2.GlancingBlows2 */;
        }

        return continueLoop;
    }
}
