using FluentAssertions;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Test.CI.Linux;

public class DesktopEntryTests
{
    [Theory]
    [InlineData("/opt/xiletrade/Xiletrade", "/opt/xiletrade/Xiletrade")]
    [InlineData("/opt/my app/Xiletrade", "\"/opt/my app/Xiletrade\"")]
    [InlineData("a$b", "\"a\\$b\"")]
    [InlineData("a\"b", "\"a\\\"b\"")]
    [InlineData("a`b", "\"a\\`b\"")]
    [InlineData("100%", "\"100%%\"")]
    [InlineData("a(b)", "\"a(b)\"")]
    [InlineData("", "\"\"")]
    public void _01_Exec_Arguments_Should_Be_Escaped(string argument, string expected)
    {
        DesktopEntry.EscapeExecArgument(argument).Should().Be(expected);
    }

    [Fact]
    public void _02_Backslash_Should_Be_Escaped_Twice()
    {
        // string-value rule then quoting rule : four characters for one
        DesktopEntry.EscapeExecArgument("a\\b").Should().Be("\"a\\\\\\\\b\"");
    }

    [Fact]
    public void _03_Entry_Should_Register_The_Url_Scheme()
    {
        var content = DesktopEntry.Build("Xiletrade", "xiletrade", "/opt/xiletrade/Xiletrade");

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Be("[Desktop Entry]");
        lines.Should().Contain("Type=Application");
        lines.Should().Contain("Name=Xiletrade");
        lines.Should().Contain("Exec=/opt/xiletrade/Xiletrade %u");
        lines.Should().Contain("NoDisplay=true");
        lines.Should().Contain("MimeType=x-scheme-handler/xiletrade;");
        content.Should().EndWith("\n");
        content.Should().NotContain("\r");
    }

    [Fact]
    public void _04_Entry_Should_Support_A_Command_With_Arguments()
    {
        var content = DesktopEntry.Build("Xiletrade", "xiletrade", "/usr/bin/dotnet", "/opt/my app/Xiletrade.dll");

        content.Should().Contain("Exec=/usr/bin/dotnet \"/opt/my app/Xiletrade.dll\" %u\n");
    }
}
