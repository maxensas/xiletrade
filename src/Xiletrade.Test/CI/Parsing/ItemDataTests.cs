using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Test.Configuration;

namespace Xiletrade.Test.CI.Parsing;

public class ItemDataTests : LibServiceConfiguration
{
    private readonly DataManagerService _poe1Dm;
    private readonly DataManagerService _poe2Dm;

    public ItemDataTests() 
    {
        // Arrange
        using var sp1 = BuildProvider();
        sp1.GetRequiredService<LocalizationService>().RefreshCurrentCulture(culture: 0);
        using var sp2 = BuildProvider();
        sp2.GetRequiredService<LocalizationService>().RefreshCurrentCulture(culture: 0);
        
        _poe1Dm = sp1.GetRequiredService<DataManagerService>();
        _poe1Dm.TryInit(forceGameVersion: 1);
        _poe2Dm = sp2.GetRequiredService<DataManagerService>();
        _poe2Dm.TryInit(forceGameVersion: 2);
    }

    [Fact]
    public void _01_PoE1_Item_Parsing_With_AllFlame_League_InfoDescription()
    {
        // Arrange
        var name = "Blight Claw";
        var type = "Wyrmscale Gauntlets";
        var descAllFlameLeague = 
            "Item Class: Gloves\r\n" +
            "Rarity: Rare\r\n" +
            "Blight Claw\r\n" +
            "Wyrmscale Gauntlets\r\n" +
            "--------\r\n" +
            "Quality: +20% (augmented)\r\n" +
            "Armour: 239 (augmented)\r\n" +
            "Evasion Rating: 239 (augmented)\r\n" +
            "Intangibility: 63%\r\n" +
            "--------\r\n" +
            "Requirements:\r\n" +
            "Level: 70\r\n" +
            "Str: 119\r\n" +
            "Dex: 68\r\n" +
            "Int: 98\r\n" +
            "--------\r\n" +
            "Sockets: W-G-B-W \r\n" +
            "--------\r\n" +
            "Item Level: 86\r\n" +
            "--------\r\n" +
            "{ Searing Exarch Implicit Modifier (Grand) }\r\n" +
            "25% chance to Unnerve Enemies for 4 seconds on Hit\r\n" +
            "(Unnerved enemies take 10% increased Spell Damage)\r\n" +
            "{ Eater of Worlds Implicit Modifier (Perfect) — Damage, Physical, Elemental, Cold }\r\n" +
            "35% of Physical Damage Converted to Cold Damage\r\n" +
            "--------\r\n" +
            "{ Prefix Modifier \"Virile\" (Tier: 2) — Life }\r\n" +
            "+113(100-114) to maximum Life\r\n" +
            "{ Prefix Modifier \"Legend's\" (Tier: 1) — Defences, Armour, Evasion }\r\n" +
            "93(92-100)% increased Armour and Evasion\r\n" +
            "{ Master Crafted Prefix Modifier \"Upgraded\" — Damage, Physical, Elemental, Cold }\r\n" +
            "25(20-25)% of Physical Damage Converted to Cold Damage\r\n" +
            "{ Suffix Modifier \"of Puhuarte\" — Damage, Elemental, Cold, Resistance }\r\n" +
            "+48(46-48)% to Cold Resistance\r\n" +
            "47(30-50)% increased Damage with Hits against Chilled Enemies\r\n" +
            "{ Suffix Modifier \"of Exile\" (Tier: 2) — Chaos, Resistance }\r\n" +
            "+30(26-30)% to Chaos Resistance\r\n" +
            "{ Suffix Modifier \"of the Storm\" (Tier: 6) — Elemental, Lightning, Resistance }\r\n" +
            "+23(18-23)% to Lightning Resistance\r\n" +
            "Searing Exarch Item\r\n" +
            "Eater of Worlds Item";

        var infoDesc = new InfoDescription(descAllFlameLeague);
        ItemData itemData = null;
        string failure = string.Empty;

        // Act
        try
        {
            if (infoDesc.IsPoeItem)
            {
                itemData = new(_poe1Dm, infoDesc);
            }
        }
        catch (Exception ex)
        {
            failure = $"{Environment.NewLine}{ex.GetBaseException()}";
        }

        // Assert
        failure.Should().BeEmpty();
        itemData.Should().NotBeNull();

        itemData.Lang.Should().Be(Library.Shared.Enum.Lang.English);
        itemData.IsPoe2.Should().BeFalse();
        itemData.Name.Should().Be(name);
        itemData.NameEn.Should().Be(name);
        itemData.NameGateway.Should().Be(name);
        itemData.Type.Should().Be(type);
        itemData.TypeEn.Should().Be(type);
        itemData.TypeGateway.Should().Be(type);

        itemData.Flag.Armour.IsArmour.Should().BeTrue();
        itemData.Flag.Armour.Gloves.Should().BeTrue();
        itemData.Flag.Socket.CanSocket.Should().BeTrue();
        itemData.Flag.Tag.ItemLevel.Should().BeTrue();
        itemData.Rarity.Should().Be("Rare");

        itemData.Options.Armour.Should().Be("239");
        itemData.Options.Evasion.Should().Be("239");
        itemData.Options.Level.Should().Be("70");
        itemData.Options.ItemLevel.Should().Be("86");
        itemData.Options.Quality.Should().Be("20");
        itemData.Options.Socket.Should().Be("W-G-B-W");

        itemData.Rule.ByBase.Should().BeFalse();
        itemData.Rule.ByType.Should().BeTrue();
        itemData.Rule.Parseable.Should().BeTrue();
        itemData.Rule.ShowDetail.Should().BeFalse();

        itemData.Stats.Life.Should().BeTrue();
        itemData.Stats.Resistance.Should().BeTrue();

        itemData.ModList.Count.Should().Be(9);

        // TO UPDATE
        itemData.ModList.Should().Contain(item => 
            item.ItemFilter.Id.EndsWith("stat_763611529") && item.ItemFilter.Text == "#% chance to Unnerve Enemies for 4 seconds on Hit");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_2133341901") && item.ItemFilter.Text == "#% of Physical Damage Converted to Cold Damage");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_3299347043") && item.ItemFilter.Text == "+# to maximum Life");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_2451402625") && item.ItemFilter.Text == "#% increased Armour and Evasion (Local)");
        //itemData.ModList.Should().Contain(item =>
            //item.ItemFilter.Id.EndsWith("stat_2133341901") && item.ItemFilter.Text == "#% of Physical Damage Converted to Cold Damage"); // duplicate
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_4220027924") && item.ItemFilter.Text == "+#% to Cold Resistance");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_2805714016") && item.ItemFilter.Text == "#% increased Damage with Hits against Chilled Enemies");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_2923486259") && item.ItemFilter.Text == "+#% to Chaos Resistance");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_1671376347") && item.ItemFilter.Text == "+#% to Lightning Resistance");
    }

    [Fact]
    public void _02_PoE2_Item_Parsing_With_Aldur_League_InfoDescription()
    {
        // Arrange
        var name = "Havoc Call";
        var type = "Sinister Quarterstaff";
        var descAldurLeague = 
            "Item Class: Quarterstaves\r\n" +
            "Rarity: Rare\r\n" +
            "Havoc Call\r\n" +
            "Sinister Quarterstaff\r\n" +
            "--------\r\n" +
            "Quality: +24% (augmented)\r\n" +
            "Physical Damage: 323-548 (augmented)\r\n" +
            "Fire Damage: 121-178 (fire)\r\n" +
            "Critical Hit Chance: 16.96% (augmented)\r\n" +
            "Attacks per Second: 1.75 (augmented)\r\n" +
            "--------\r\n" +
            "Requires: Level 67, 104 Dex, 41 Int\r\n" +
            "--------\r\n" +
            "Sockets: S S \r\n" +
            "--------\r\n" +
            "Item Level: 79\r\n" +
            "--------\r\n" +
            "50% increased Attack Damage against Rare or Unique Enemies (rune)\r\n" +
            "Gain 5% of Damage as Extra Damage of all Elements (rune)\r\n" +
            "--------\r\n" +
            "{ Prefix Modifier \"Tyrannical\" (Tier: 2) — Damage, Physical, Attack }\r\n" +
            "166(155-169)% increased Physical Damage\r\n" +
            "{ Prefix Modifier \"Cremating\" (Tier: 2) — Damage, Elemental, Fire, Attack }\r\n" +
            "Adds 121(102-130) to 178(155-198) Fire Damage\r\n" +
            "{ Prefix Modifier \"Tempered\" (Tier: 2) — Damage, Physical, Attack }\r\n" +
            "Adds 43(29-44) to 75(50-75) Physical Damage\r\n" +
            "{ Suffix Modifier \"of Unmaking\" (Tier: 1) — Attack, Critical }\r\n" +
            "+4.96(4.41-5)% to Critical Hit Chance\r\n" +
            "{ Desecrated Suffix Modifier \"of Infamy\" (Tier: 2) — Attack, Speed }\r\n" +
            "25(23-25)% increased Attack Speed\r\n" +
            "{ Crafted Suffix Modifier \"of the Essence\" — Attack }\r\n" +
            "+3 to Level of all Attack Skills\r\n";

        var infoDesc = new InfoDescription(descAldurLeague);
        ItemData itemData = null;
        string failure = string.Empty;

        // Act
        try
        {
            if (infoDesc.IsPoeItem)
            {
                itemData = new(_poe2Dm, infoDesc);
            }
        }
        catch (Exception ex)
        {
            failure = $"{Environment.NewLine}{ex.GetBaseException()}";
        }

        // Assert
        itemData.Should().NotBeNull();
        failure.Should().BeEmpty();

        itemData.Lang.Should().Be(Library.Shared.Enum.Lang.English);
        itemData.IsPoe2.Should().BeTrue();
        itemData.Name.Should().Be(name);
        itemData.NameEn.Should().Be(name);
        itemData.NameGateway.Should().Be(name);
        itemData.Type.Should().Be(type);
        itemData.TypeEn.Should().Be(type);
        itemData.TypeGateway.Should().Be(type);
        
        itemData.Flag.Weapon.IsWeapon.Should().BeTrue();
        itemData.Flag.Weapon.IsStave.Should().BeTrue();
        itemData.Flag.Weapon.QuarterStaff.Should().BeTrue();
        itemData.Flag.Socket.CanSocket.Should().BeTrue();
        itemData.Flag.Tag.ItemLevel.Should().BeTrue();
        itemData.Rarity.Should().Be("Rare");

        itemData.Options.AttacksPerSecond.Should().Be("1.75");
        itemData.Options.CriticalHitChance.Should().Be("16.96");
        itemData.Options.FireDamage.Should().Be("121-178 (fire)");
        itemData.Options.ItemLevel.Should().Be("79");
        itemData.Options.Quality.Should().Be("24");
        itemData.Options.Socket.Should().Be("S S");

        itemData.Damage.PysicalMin.Should().Be("762");
        itemData.Damage.ElementalMin.Should().Be("261");
        itemData.Damage.TotalMin.Should().Be("1023");

        itemData.Rule.ByBase.Should().BeFalse();
        itemData.Rule.ByType.Should().BeTrue();
        itemData.Rule.Parseable.Should().BeTrue();
        itemData.Rule.ShowDetail.Should().BeFalse();

        itemData.ModList.Count.Should().Be(8);

        //TO UPDATE
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_2077615515") && item.ItemFilter.Text == "#% increased Attack Damage against Rare or Unique Enemies");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_731403740") && item.ItemFilter.Text == "Gain #% of Damage as Extra Damage of all Elements");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_1509134228") && item.ItemFilter.Text == "#% increased Physical Damage");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_709508406") && item.ItemFilter.Text == "Adds # to # Fire Damage");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_1940865751") && item.ItemFilter.Text == "Adds # to # Physical Damage");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_518292764") && item.ItemFilter.Text == "#% to Critical Hit Chance");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_210067635") && item.ItemFilter.Text == "#% increased Attack Speed (Local)");
        itemData.ModList.Should().Contain(item =>
            item.ItemFilter.Id.EndsWith("stat_3035140377") && item.ItemFilter.Text == "# to Level of all Attack Skills");
    }
}
