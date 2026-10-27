using FluentAssertions;
using System.Text;
using Xiletrade.DatToJson;

namespace Xiletrade.Test.CI.SchemaCalibration;

public class JsonDiffTests
{
    [Fact]
    public void _01_Changed_Entries_Should_Give_A_Git_Unified_Diff_With_One_Entry_Per_Line_Without_Context()
    {
        // Arrange: 10 entries, the 5th one changes.
        var before = Mods(Enumerable.Range(1, 10).Select(i => $"{{\"id\":\"M{i}\"}}"));
        var after = Mods(Enumerable.Range(1, 10).Select(i => i == 5 ? "{\"id\":\"M5\",\"level\":\"2\"}" : $"{{\"id\":\"M{i}\"}}"));
        using var output = new MemoryStream();

        // Act
        var stats = JsonDiff.Compare(before, after, output, "en-US/Mods.json", newFile: false);

        // Assert
        stats.Should().Be(new JsonDiff.Stats(1, 1));
        Encoding.UTF8.GetString(output.ToArray()).Should().Be(
            "diff --git a/en-US/Mods.json b/en-US/Mods.json\n" +
            "--- a/en-US/Mods.json\n" +
            "+++ b/en-US/Mods.json\n" +
            "@@ -5 +5 @@\n" +
            "-{\"id\":\"M5\"}\n" +
            "+{\"id\":\"M5\",\"level\":\"2\"}\n");
    }

    [Fact]
    public void _02_Distant_Changes_Should_Give_Separate_Hunks()
    {
        // Arrange: first and last of 20 entries removed.
        var all = Enumerable.Range(1, 20).Select(i => $"{{\"id\":\"M{i}\"}}").ToArray();
        using var output = new MemoryStream();

        // Act
        var stats = JsonDiff.Compare(Mods(all), Mods(all[1..^1]), output, "f.json", newFile: false);

        // Assert
        stats.Should().Be(new JsonDiff.Stats(2, 0));
        Encoding.UTF8.GetString(output.ToArray()).Should().Be(
            "diff --git a/f.json b/f.json\n" +
            "--- a/f.json\n" +
            "+++ b/f.json\n" +
            "@@ -1 +0,0 @@\n" +
            "-{\"id\":\"M1\"}\n" +
            "@@ -20 +18,0 @@\n" +
            "-{\"id\":\"M20\"}\n");
    }

    [Fact]
    public void _03_New_File_Should_Be_Shown_As_Added()
    {
        // Arrange
        var json = ItemClasses("{\"id\":0,\"name\":\"Life Flasks\"}", "{\"id\":1,\"name\":\"Mana Flasks\"}");
        using var output = new MemoryStream();

        // Act
        var stats = JsonDiff.Compare(ReadOnlyMemory<byte>.Empty, json, output, "fr-FR/ItemClass.json", newFile: true);

        // Assert
        stats.Should().Be(new JsonDiff.Stats(0, 2));
        Encoding.UTF8.GetString(output.ToArray()).Should().Be(
            "diff --git a/fr-FR/ItemClass.json b/fr-FR/ItemClass.json\n" +
            "new file mode 100644\n" +
            "--- /dev/null\n" +
            "+++ b/fr-FR/ItemClass.json\n" +
            "@@ -0,0 +1,2 @@\n" +
            "+{\"id\":0,\"name\":\"Life Flasks\"}\n" +
            "+{\"id\":1,\"name\":\"Mana Flasks\"}\n");
    }

    [Fact]
    public void _04_Identical_File_Should_Not_Be_Written_Again()
    {
        // Arrange
        var folder = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(folder, "en-US", "Mods.json");
        var json = Mods(["{\"id\":\"M1\"}"]);
        try
        {
            new JsonChanges(null).Write(folder, path, json);
            var written = File.GetLastWriteTimeUtc(path);
            File.SetLastWriteTimeUtc(path, written.AddHours(-1));
            var changes = new JsonChanges(null);

            // Act
            changes.Write(folder, path, json);

            // Assert
            changes.Files.Should().ContainSingle().Which.Status.Should().Be(JsonFileStatus.Unchanged);
            File.GetLastWriteTimeUtc(path).Should().Be(written.AddHours(-1));
            File.ReadAllBytes(path).AsSpan(0, 3).ToArray().Should().Equal(Encoding.UTF8.Preamble.ToArray());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static byte[] Mods(IEnumerable<string> entries)
        => Encoding.UTF8.GetBytes("{\"result\":[{\"data\":[" + string.Join(',', entries) + "]}]}");

    private static byte[] ItemClasses(params string[] entries)
        => Encoding.UTF8.GetBytes("{\"item_class\":[" + string.Join(',', entries) + "]}");
}
