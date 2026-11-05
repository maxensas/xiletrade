using FluentAssertions;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Shared;

namespace Xiletrade.Test.CI.Parsing;

/// <summary>
/// The parsing rules of every language (Data/Lang/*/ParsingRules.json) must pass the checks done when they are loaded (01 to 03),
/// and must not contain mistakes that do not stop the loading but make a rule useless or wrong (04 to 06).
/// </summary>
public class ParsingRulesTests
{
    public static IEnumerable<string[]> Cultures()
    {
        foreach (var culture in Strings.Culture)
        {
            yield return [ culture ];
        }
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void _01_Rule_Files_Should_Be_Valid(string culture)
    {
        var fileName = Path.Combine("Lang", culture, Strings.File.ParsingRules);
        var parser = LoadRules(culture);

        parser.Mods.Should().NotBeNull();
        FluentActions.Invoking(() => parser.Validate(fileName)).Should().NotThrow();
    }

    [Theory]
    [InlineData("equal", "old", "new")]
    [InlineData("", "old", "new")]
    [InlineData("contains", "", "new")]
    [InlineData("equals", "old", "")]
    public void _02_Validate_Should_Reject_Invalid_Rules(string replace, string old, string @new)
    {
        var parser = new ParserData { Mods = [ new() { Replace = replace, Old = old, New = @new } ] };
        FluentActions.Invoking(() => parser.Validate("test.json")).Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void _03_Validate_Should_Reject_Missing_Mods()
    {
        FluentActions.Invoking(() => new ParserData().Validate("test.json")).Should().Throw<InvalidDataException>();
    }

    // only the first rule that fits is used : a second rule with the same replace and old is never used
    [Theory]
    [MemberData(nameof(Cultures))]
    public void _04_Rules_Should_Not_Be_Duplicated(string culture)
    {
        var mods = LoadRules(culture).Mods;
        var errors = new List<string>();
        for (int j = 0; j < mods.Length; j++)
        {
            for (int i = 0; i < j; i++)
            {
                if (!mods[i].Disabled && !mods[j].Disabled && IsSameRule(mods[i], mods[j]))
                {
                    errors.Add($"id {mods[j].Id} is the same rule as id {mods[i].Id} : '{mods[j].Old}'");
                    break;
                }
            }
        }
        errors.Should().BeEmpty($"each rule of {culture} must be written once");
    }

    // a rule placed after a 'contains' rule whose old is a part of its own old is never used :
    // every mod that fits the second rule fits the first one before
    [Theory]
    [MemberData(nameof(Cultures))]
    public void _05_Rules_Should_Not_Be_Hidden_By_A_Previous_Rule(string culture)
    {
        var mods = LoadRules(culture).Mods;
        var errors = new List<string>();
        for (int j = 0; j < mods.Length; j++)
        {
            for (int i = 0; i < j; i++)
            {
                var first = mods[i];
                var next = mods[j];
                if (first.Disabled || next.Disabled || IsSameRule(first, next)) // duplicates : see _04
                {
                    continue;
                }
                if (first.Replace is Strings.Word.contains && next.Old.Contains(first.Old, StringComparison.Ordinal))
                {
                    errors.Add($"id {next.Id} '{next.Old}' is hidden by id {first.Id} '{first.Old}' ({first.Replace})");
                    break;
                }
            }
        }
        errors.Should().BeEmpty($"every rule of {culture} must be reachable");
    }

    // the id is not used by the parsing, but it is the way to name a rule (issues, pull requests)
    [Theory]
    [MemberData(nameof(Cultures))]
    public void _06_Ids_Should_Be_Unique(string culture)
    {
        var duplicates = LoadRules(culture).Mods
            .GroupBy(mod => mod.Id)
            .Where(group => group.Count() > 1)
            .Select(group => $"id {group.Key} : {string.Join(" / ", group.Select(mod => $"'{mod.Old}'"))}");
        duplicates.Should().BeEmpty($"each rule of {culture} must have its own id");
    }

    private static bool IsSameRule(ModOption first, ModOption next)
        => first.Replace == next.Replace && first.Old == next.Old;

    private static ParserData LoadRules(string culture)
    {
        using var stream = File.OpenRead(Path.Combine(GetDataPath(), "Lang", culture, Strings.File.ParsingRules));
        return new JsonHelper().Deserialize<ParserData>(stream);
    }

    private static string GetDataPath()
    {
        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = basePath[..basePath.IndexOf("bin")];
        return Path.Combine(basePath, "Data");
    }
}
