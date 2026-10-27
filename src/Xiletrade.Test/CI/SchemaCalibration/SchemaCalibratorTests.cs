using FluentAssertions;
using Xiletrade.DatToJson;
using static Xiletrade.Test.CI.SchemaCalibration.TestField;

namespace Xiletrade.Test.CI.SchemaCalibration;

/// <summary>
/// Simulates game updates on synthetic dat files: the snapshot is taken on the "before" file,
/// the calibration runs on the "after" file.
/// </summary>
public class SchemaCalibratorTests
{
    private const string Mods = "mods";

    // Same idea as mods: key, used columns, and unused columns that look alike (decoys).
    private static readonly TestField[] ModsBefore =
    [
        Str("Id"), Int("Hash"), Pad("Pad1", 14), Int("Level"), Str("Decoy"), Int("DecoyInt"),
        Pad("Pad2", 20), Str("Name"), Pad("Pad3", 9)
    ];

    [Fact]
    public void _01_Same_Structure_With_New_And_Reworded_Rows_Should_Keep_Offsets()
    {
        // Act
        var result = CalibrateMods(ModsBefore);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Tables[0].Columns.Should().OnlyContain(c => c.Status == ColumnStatus.Unchanged);
        AssertOffsets(result, Mods, ModsBefore, "Id", "Level", "Name");
    }

    [Fact]
    public void _02_Column_Inserted_Before_Name_Should_Move_Name()
    {
        // Arrange
        TestField[] after = [.. ModsBefore[..7], Pad("New", 16), .. ModsBefore[7..]];

        // Act
        var result = CalibrateMods(after);

        // Assert
        result.Succeeded.Should().BeTrue();
        Column(result, Mods, "Name").Status.Should().Be(ColumnStatus.Moved);
        AssertOffsets(result, Mods, after, "Id", "Level", "Name");
        result.Schema.Tables[0].RowLength.Should().Be(DatTestBuilder.RowLength(after));
    }

    [Fact]
    public void _03_Byte_Inserted_Before_Key_Should_Move_Every_Column()
    {
        // Arrange
        TestField[] after = [Pad("Flag", 1), .. ModsBefore];

        // Act
        var result = CalibrateMods(after);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Tables[0].Columns.Should().OnlyContain(c => c.Status == ColumnStatus.Moved);
        AssertOffsets(result, Mods, after, "Id", "Level", "Name");
    }

    [Fact]
    public void _04_Two_Insertions_Should_Move_Each_Column()
    {
        // Arrange
        TestField[] after = [.. ModsBefore[..3], Pad("A", 4), .. ModsBefore[3..7], Pad("B", 24), .. ModsBefore[7..]];

        // Act
        var result = CalibrateMods(after);

        // Assert
        result.Succeeded.Should().BeTrue();
        AssertOffsets(result, Mods, after, "Id", "Level", "Name");
    }

    [Fact]
    public void _05_Name_Becoming_An_Integer_Should_Fail_Without_Writing()
    {
        // Arrange
        TestField[] after = [.. ModsBefore.Select(f => f.Name == "Name" ? Int("Name") : f)];
        var rows = ModRows(5500, rewordEvery: 40);
        for (var i = 0; i < rows.Count; i++)
        {
            rows[i]["Name"] = i;
        }

        // Act
        var result = Calibrate(
            new GameSchema { Game = 1, Tables = [DatTestBuilder.Table(Mods, ModsBefore, "Id", "Level", "Name")] },
            new() { [Mods] = DatTestBuilder.Dat(ModsBefore, ModRows(5000)) },
            new() { [Mods] = DatTestBuilder.Dat(after, rows) });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Schema.Should().BeNull();
        result.Snapshot.Should().BeNull();
        Column(result, Mods, "Name").Status.Should().Be(ColumnStatus.Failed);
    }

    [Fact]
    public void _06_Removed_Name_Should_Fail()
    {
        // Arrange
        TestField[] after = [.. ModsBefore.Where(f => f.Name != "Name")];

        // Act
        var result = CalibrateMods(after);

        // Assert
        result.Succeeded.Should().BeFalse();
        Column(result, Mods, "Name").Status.Should().Be(ColumnStatus.Failed);
    }

    [Fact]
    public void _07_Identical_Text_Columns_Should_Keep_Their_Order()
    {
        // Arrange: in english words, Text and Text2 hold the same values.
        TestField[] before = [Int("List"), Str("Text"), Pad("Pad", 36), Str("Text2"), Pad("Pad2", 8)];
        TestField[] after = [Pad("New", 8), .. before[..3], Pad("New2", 16), .. before[3..]];
        var rows = Enumerable.Range(0, 3000)
            .Select(i => new Dictionary<string, object> { ["List"] = i % 6, ["Text"] = $"Word {i}", ["Text2"] = $"Word {i}" })
            .ToList();
        var schema = new GameSchema { Game = 1, Tables = [DatTestBuilder.Table("words", before, "Text", "Text2")] };

        // Act
        var result = Calibrate(schema,
            new() { ["words"] = DatTestBuilder.Dat(before, rows) },
            new() { ["words"] = DatTestBuilder.Dat(after, rows) });

        // Assert
        result.Succeeded.Should().BeTrue();
        AssertOffsets(result, "words", after, "Text", "Text2");
        result.HasWarnings.Should().BeTrue("Text2 also matches the Text values, the choice must be reported");
    }

    [Fact]
    public void _08_Foreign_Row_Should_Be_Compared_Through_Referenced_Keys()
    {
        // Arrange: two item classes inserted at the start, so every class index shifts by 2.
        TestField[] classes = [Str("Id"), Str("Name"), Pad("Pad", 10)];
        TestField[] basesBefore = [Str("Id"), Foreign("Class"), Pad("Pad", 8), Str("Name"), Pad("Pad2", 30)];
        TestField[] basesAfter = [basesBefore[0], Pad("Flag", 2), .. basesBefore[1..]];

        var classRows = Enumerable.Range(0, 60)
            .Select(i => new Dictionary<string, object> { ["Id"] = $"Class{i}", ["Name"] = $"Class name {i}" })
            .ToList();
        var classRowsAfter = new List<Dictionary<string, object>>
        {
            new() { ["Id"] = "ClassNewA", ["Name"] = "New A" },
            new() { ["Id"] = "ClassNewB", ["Name"] = "New B" }
        };
        classRowsAfter.AddRange(classRows);

        List<Dictionary<string, object>> baseRows = [], baseRowsAfter = [];
        for (var i = 0; i < 2000; i++)
        {
            var row = new Dictionary<string, object> { ["Id"] = $"Base{i}", ["Name"] = $"Base name {i}" };
            var rowAfter = new Dictionary<string, object>(row);
            if (i % 11 != 0)
            {
                row["Class"] = i % 60;
                rowAfter["Class"] = i % 60 + 2;
            }
            baseRows.Add(row);
            baseRowsAfter.Add(rowAfter);
        }

        var bases = DatTestBuilder.Table("baseitemtypes", basesBefore, "Id", "Class", "Name");
        bases.FindColumn("Class").References = "itemclasses";
        var schema = new GameSchema
        {
            Game = 1,
            Tables = [DatTestBuilder.Table("itemclasses", classes, "Id", "Name"), bases]
        };

        // Act
        var result = Calibrate(schema,
            new()
            {
                ["itemclasses"] = DatTestBuilder.Dat(classes, classRows),
                ["baseitemtypes"] = DatTestBuilder.Dat(basesBefore, baseRows)
            },
            new()
            {
                ["itemclasses"] = DatTestBuilder.Dat(classes, classRowsAfter),
                ["baseitemtypes"] = DatTestBuilder.Dat(basesAfter, baseRowsAfter)
            });

        // Assert
        result.Succeeded.Should().BeTrue();
        AssertOffsets(result, "baseitemtypes", basesAfter, "Id", "Class", "Name");
    }

    [Fact]
    public void _09_Snapshot_Saved_Then_Loaded_Should_Give_Same_Values()
    {
        // Arrange
        var schema = new GameSchema { Game = 2, Tables = [DatTestBuilder.Table(Mods, ModsBefore, "Id", "Level", "Name")] };
        List<string> errors = [];
        var snapshot = SchemaCalibrator.BuildSnapshot(schema,
            new Dictionary<string, DatFile> { [Mods] = DatTestBuilder.Dat(ModsBefore, ModRows(1000)) }, errors);
        var path = Path.GetTempFileName();

        try
        {
            // Act
            snapshot.Save(path);
            var loaded = DatSnapshot.Load(path);

            // Assert
            errors.Should().BeEmpty();
            loaded.Game.Should().Be(2);
            var table = loaded.Tables[Mods];
            table.RowCount.Should().Be(1000);
            foreach (var (name, values) in snapshot.Tables[Mods].Columns)
            {
                table.Columns[name].Should().Equal(values);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void _10_Snapshot_With_Wrong_Offset_Should_Report_An_Error()
    {
        // Arrange: Name declared on the Level offset, which is not a string.
        var table = DatTestBuilder.Table(Mods, ModsBefore, "Id", "Level", "Name");
        table.FindColumn("Name").Offset = DatTestBuilder.OffsetOf(ModsBefore, "Level");
        var schema = new GameSchema { Game = 1, Tables = [table] };
        List<string> errors = [];

        // Act
        var snapshot = SchemaCalibrator.BuildSnapshot(schema,
            new Dictionary<string, DatFile> { [Mods] = DatTestBuilder.Dat(ModsBefore, ModRows(100)) }, errors);

        // Assert
        snapshot.Should().BeNull();
        errors.Should().ContainSingle().Which.Should().Contain("mods.Name");
    }

    private static List<Dictionary<string, object>> ModRows(int count, int rewordEvery = 0)
    {
        var random = new Random(42);
        var rows = new List<Dictionary<string, object>>(count);
        for (var i = 0; i < count; i++)
        {
            var name = i % 3 == 0 ? string.Empty : $"Name of mod {i}";
            if (rewordEvery > 0 && i % rewordEvery == 0 && name.Length > 0)
            {
                name += " (reworded)";
            }
            rows.Add(new Dictionary<string, object>
            {
                ["Id"] = $"Mod{i}",
                ["Hash"] = random.Next(0, 65536),
                ["Level"] = i % 80 + 1,
                ["Decoy"] = $"Decoy {i % 50}",
                ["DecoyInt"] = i % 7,
                ["Name"] = name
            });
        }
        return rows;
    }

    /// <summary>5000 rows before; 5500 rows after (10% new) with some names reworded.</summary>
    private static CalibrationResult CalibrateMods(TestField[] after)
        => Calibrate(
            new GameSchema { Game = 1, Tables = [DatTestBuilder.Table(Mods, ModsBefore, "Id", "Level", "Name")] },
            new() { [Mods] = DatTestBuilder.Dat(ModsBefore, ModRows(5000)) },
            new() { [Mods] = DatTestBuilder.Dat(after, ModRows(5500, rewordEvery: 40)) });

    private static CalibrationResult Calibrate(GameSchema schema, Dictionary<string, DatFile> before, Dictionary<string, DatFile> after)
    {
        List<string> errors = [];
        var snapshot = SchemaCalibrator.BuildSnapshot(schema, before, errors);
        errors.Should().BeEmpty();
        return new SchemaCalibrator(new CalibrationOptions()).Calibrate(schema, snapshot, after);
    }

    private static ColumnResult Column(CalibrationResult result, string table, string column)
        => result.Tables.Single(t => t.Name == table).Columns.Single(c => c.Name == column);

    private static void AssertOffsets(CalibrationResult result, string table, TestField[] fields, params string[] columns)
    {
        var schema = result.Schema.Tables.Single(t => t.Name == table);
        foreach (var column in columns)
        {
            schema.FindColumn(column).Offset.Should().Be(DatTestBuilder.OffsetOf(fields, column), column);
        }
    }
}
