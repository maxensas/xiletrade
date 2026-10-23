using FluentAssertions;
using Xiletrade.Library.Services.Linux.Helper;

namespace Xiletrade.Test.CI.Linux;

public class ProcessRunnerTests
{
    [Fact]
    public void _01_Missing_Tool_Should_Not_Throw()
    {
        var result = ProcessRunner.Run("/nonexistent/xiletrade-tool", ["--version"]);

        result.Success.Should().BeFalse();
        result.ExitCode.Should().Be(-1);
        result.TimedOut.Should().BeFalse();
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void _02_Missing_Tool_Without_Output_Capture_Should_Not_Throw()
    {
        var result = ProcessRunner.Run("/nonexistent/xiletrade-tool", [], stdin: "text", captureOutput: false);

        result.Success.Should().BeFalse();
    }
}
