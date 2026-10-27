using FluentAssertions;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Test.CI.Linux;

public sealed class ExecutableLocatorTests : IDisposable
{
    private readonly string _first = Directory.CreateTempSubdirectory("xiletrade-path1-").FullName;
    private readonly string _second = Directory.CreateTempSubdirectory("xiletrade-path2-").FullName;

    public void Dispose()
    {
        Directory.Delete(_first, recursive: true);
        Directory.Delete(_second, recursive: true);
    }

    private static string PathVariable(params string[] directories)
        => string.Join(System.IO.Path.PathSeparator, directories);

    [Fact]
    public void _01_Should_Find_The_Tool_In_The_Path()
    {
        var tool = System.IO.Path.Combine(_second, "xclip");
        File.WriteAllText(tool, string.Empty);

        ExecutableLocator.Find("xclip", PathVariable(_first, _second)).Should().Be(tool);
    }

    [Fact]
    public void _02_Should_Return_The_First_Match_Of_The_Path()
    {
        var firstTool = System.IO.Path.Combine(_first, "xsel");
        File.WriteAllText(firstTool, string.Empty);
        File.WriteAllText(System.IO.Path.Combine(_second, "xsel"), string.Empty);

        ExecutableLocator.Find("xsel", PathVariable(_first, _second)).Should().Be(firstTool);
    }

    [Fact]
    public void _03_Should_Return_Null_When_The_Tool_Is_Missing()
    {
        ExecutableLocator.Find("not-installed-tool", PathVariable(_first, _second)).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../xclip")]
    [InlineData("/usr/bin/xclip")]
    public void _04_Should_Reject_Names_Which_Are_Not_File_Names(string name)
    {
        ExecutableLocator.Find(name, PathVariable(_first, _second)).Should().BeNull();
    }

    [Fact]
    public void _05_Should_Cope_With_An_Empty_Path()
    {
        ExecutableLocator.Find("xclip", string.Empty).Should().BeNull();
        ExecutableLocator.Find("xclip", new string(System.IO.Path.PathSeparator, 2)).Should().BeNull();
    }
}
