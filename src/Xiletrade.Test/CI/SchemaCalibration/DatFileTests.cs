using FluentAssertions;
using Xiletrade.DatToJson;
using static Xiletrade.Test.CI.SchemaCalibration.TestField;

namespace Xiletrade.Test.CI.SchemaCalibration;

public class DatFileTests
{
    [Fact]
    public void _01_Parse_Should_Compute_Row_Length_And_Read_Every_Type()
    {
        // Arrange
        TestField[] fields = [Str("Id"), Int("Level"), Foreign("Class"), Pad("Unused", 3)];
        Dictionary<string, object>[] rows =
        [
            new() { ["Id"] = "First", ["Level"] = 42, ["Class"] = 7 },
            new() { ["Id"] = "Second", ["Level"] = -1 },
            new() { ["Id"] = "" }
        ];

        // Act
        var dat = DatTestBuilder.Dat(fields, rows);

        // Assert
        dat.RowCount.Should().Be(3);
        dat.RowLength.Should().Be(8 + 4 + 16 + 3);
        dat.ReadString(0, 0).Should().Be("First");
        dat.ReadString(1, 0).Should().Be("Second");
        dat.ReadString(2, 0).Should().BeEmpty();
        dat.ReadInt32(0, 8).Should().Be(42);
        dat.ReadInt32(1, 8).Should().Be(-1);
        dat.TryReadForeignRow(0, 12, out var index).Should().BeTrue();
        index.Should().Be(7UL);
        dat.TryReadForeignRow(1, 12, out index).Should().BeTrue();
        index.Should().Be(DatFile.NullRow);
    }

    [Fact]
    public void _02_Marker_Bytes_Inside_Rows_Should_Be_Ignored()
    {
        // Arrange: 8 bytes of 0xBB in row 1, where the variable section can not begin.
        TestField[] fields = [Str("Id"), Pad("Raw", 8), Pad("Unused", 5)];
        Dictionary<string, object>[] rows =
        [
            new() { ["Id"] = "a" },
            new() { ["Id"] = "b", ["Raw"] = new byte[] { 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB } },
            new() { ["Id"] = "c" },
            new() { ["Id"] = "d" }
        ];

        // Act
        var dat = DatTestBuilder.Dat(fields, rows);

        // Assert
        dat.RowCount.Should().Be(4);
        dat.RowLength.Should().Be(21);
        dat.ReadString(3, 0).Should().Be("d");
    }

    [Fact]
    public void _03_Bytes_That_Are_Not_A_String_Reference_Should_Be_Rejected()
    {
        // Arrange: an integer column read as a string points outside the variable section.
        TestField[] fields = [Str("Id"), Int("A"), Int("B")];
        Dictionary<string, object>[] rows = [new() { ["Id"] = "x", ["A"] = 123456, ["B"] = 654321 }];
        var dat = DatTestBuilder.Dat(fields, rows);

        // Act
        var valid = dat.TryReadString(0, DatTestBuilder.OffsetOf(fields, "A"), out _);

        // Assert
        valid.Should().BeFalse();
    }
}
