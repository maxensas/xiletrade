using FluentAssertions;
using System.Text.Json;
using Xiletrade.DatToJson;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using static Xiletrade.Test.CI.SchemaCalibration.TestField;

namespace Xiletrade.Test.CI.SchemaCalibration;

public class JsonExporterTests
{
    private static readonly TestField[] ModsFields = [Str("Id"), Pad("Pad", 18), Int("Level"), Pad("Pad2", 12), Str("Name"), Pad("Pad3", 5)];

    [Fact]
    public void _01_Mods_Export_Should_Apply_Rules_And_English_Names()
    {
        // Arrange
        Dictionary<string, object>[] english =
        [
            new() { ["Id"] = "Strength1", ["Level"] = 1, ["Name"] = "of the Brute" },
            new() { ["Id"] = "Strength2", ["Level"] = 11, ["Name"] = "of the Brute" },          // same name: dropped
            new() { ["Id"] = "NoName", ["Level"] = 5, ["Name"] = "" },                          // no name: dropped
            new() { ["Id"] = "EssenceLife", ["Level"] = 10, ["Name"] = "Essence mod" },         // unwanted id: dropped
            new() { ["Id"] = "Dexterity1", ["Level"] = 3, ["Name"] = "<if:MS>{du Félin}<elif:FS>{de la Féline}" }
        ];
        Dictionary<string, object>[] french =
        [
            new() { ["Id"] = "Strength1", ["Level"] = 1, ["Name"] = "de la Brute" },
            new() { ["Id"] = "FrenchOnly", ["Level"] = 1, ["Name"] = "Inconnu" },               // not in english: dropped
            new() { ["Id"] = "Dexterity1", ["Level"] = 3, ["Name"] = "<if:MS>{du Félin}<elif:FS>{de la Féline}" }
        ];
        var source = new MemoryDatSource
        {
            ["data/balance/mods.datc64"] = DatTestBuilder.Build(ModsFields, english),
            ["data/balance/french/mods.datc64"] = DatTestBuilder.Build(ModsFields, french)
        };
        var schema = new GameSchema { Game = 2, Tables = [DatTestBuilder.Table("mods", ModsFields, "Id", "Level", "Name")] };
        var output = Directory.CreateTempSubdirectory().FullName;

        try
        {
            // Act
            var result = new JsonExporter(schema, source, output, new JsonChanges(null)).Export();

            // Assert
            result.Files.Should().BeEquivalentTo(new[] { Path.Combine("en-US", "ModsTwo.json"), Path.Combine("fr-FR", "ModsTwo.json") });

            var en = ReadMods(Path.Combine(output, "en-US", "ModsTwo.json"));
            en.Select(m => m.Id).Should().Equal("Strength1", "Dexterity1");
            en[0].Level.Should().Be("1");
            en[1].Name.Should().Be("du Félin/de la Féline");

            var fr = ReadMods(Path.Combine(output, "fr-FR", "ModsTwo.json"));
            fr.Select(m => m.Id).Should().Equal("Strength1", "Dexterity1");
            fr[0].Name.Should().Be("de la Brute");
            fr[0].NameEn.Should().Be("of the Brute");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Fact]
    public void _02_Row_Length_Different_From_Schema_Should_Throw()
    {
        // Arrange: the game added a column, the schema was not calibrated.
        TestField[] newer = [.. ModsFields, Pad("New", 4)];
        Dictionary<string, object>[] rows = [new() { ["Id"] = "Strength1", ["Level"] = 1, ["Name"] = "of the Brute" }];
        var source = new MemoryDatSource { ["data/balance/mods.datc64"] = DatTestBuilder.Build(newer, rows) };
        var schema = new GameSchema { Game = 2, Tables = [DatTestBuilder.Table("mods", ModsFields, "Id", "Level", "Name")] };
        var output = Directory.CreateTempSubdirectory().FullName;

        try
        {
            // Act
            var act = () => new JsonExporter(schema, source, output, new JsonChanges(null)).Export();

            // Assert
            act.Should().Throw<InvalidDataException>().WithMessage("*calibrate*");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Theory]
    [InlineData("<if:MS>{Brutal}<elif:FS>{Brutale}<elif:MP>{Brutaux}", "Brutal/Brutale/Brutaux")]
    [InlineData("<if:MS>{Rouge}<elif:FS>{Rouge}", "Rouge")]
    [InlineData("  Plain name ", "Plain name")]
    public void _03_Grammatical_Forms_Should_Be_Joined(string source, string expected)
        => ExportRules.ParseMultipleName(source).Should().Be(expected);

    [Fact]
    public void _04_Gem_Type_From_Id_Should_Add_Spaces_Before_Capitals()
        => ExportRules.GemTypeFromId("ArcticArmourAltX").Should().Be("Arctic Armour");

    [Fact]
    public void _05_Second_Export_Should_Report_Only_The_Changed_Entries()
    {
        // Arrange: a first generation, then the game removes a mod and adds another one.
        Dictionary<string, object>[] before =
        [
            new() { ["Id"] = "Strength1", ["Level"] = 1, ["Name"] = "of the Brute" },
            new() { ["Id"] = "Strength2", ["Level"] = 11, ["Name"] = "of the Wrestler" },
            new() { ["Id"] = "Strength3", ["Level"] = 22, ["Name"] = "of the Bear" }
        ];
        Dictionary<string, object>[] after =
        [
            new() { ["Id"] = "Strength1", ["Level"] = 1, ["Name"] = "of the Brute" },
            new() { ["Id"] = "Strength3", ["Level"] = 22, ["Name"] = "of the Bear" },
            new() { ["Id"] = "Strength4", ["Level"] = 33, ["Name"] = "of the Lion" }
        ];
        var schema = new GameSchema { Game = 2, Tables = [DatTestBuilder.Table("mods", ModsFields, "Id", "Level", "Name")] };
        var output = Directory.CreateTempSubdirectory().FullName;

        try
        {
            new JsonExporter(schema, new MemoryDatSource { ["data/balance/mods.datc64"] = DatTestBuilder.Build(ModsFields, before) },
                output, new JsonChanges(null)).Export();
            using var diff = new MemoryStream();
            var changes = new JsonChanges(diff);

            // Act
            new JsonExporter(schema, new MemoryDatSource { ["data/balance/mods.datc64"] = DatTestBuilder.Build(ModsFields, after) },
                output, changes).Export();

            // Assert
            changes.Files.Should().ContainSingle().Which.Should().Be(new JsonFileChange("en-US/ModsTwo.json", JsonFileStatus.Changed, 1, 1));
            var text = System.Text.Encoding.UTF8.GetString(diff.ToArray());
            text.Should().StartWith("diff --git a/en-US/ModsTwo.json b/en-US/ModsTwo.json\n--- a/en-US/ModsTwo.json\n+++ b/en-US/ModsTwo.json\n");
            text.Should().Contain("\n-{\"id\":\"Strength2\"").And.Contain("\n+{\"id\":\"Strength4\"");
            ReadMods(Path.Combine(output, "en-US", "ModsTwo.json")).Select(m => m.Id).Should().Equal("Strength1", "Strength3", "Strength4");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static ModResultData[] ReadMods(string path)
        => JsonSerializer.Deserialize<ModData>(File.ReadAllText(path)).Result[0].Data;

    private sealed class MemoryDatSource : Dictionary<string, byte[]>, IDatSource
    {
        public string Description => "memory";
        public byte[] Read(string path) => TryGetValue(path, out var data) ? data : null;
        public bool Exists(string path) => ContainsKey(path);
        public void Dispose() { }
    }
}
