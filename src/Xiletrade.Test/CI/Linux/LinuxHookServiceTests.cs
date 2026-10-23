using System.Runtime.Versioning;
using FluentAssertions;
using Xiletrade.Library.Services.Linux;

namespace Xiletrade.Test.CI.Linux;

[SupportedOSPlatform("linux")]
public class LinuxHookServiceTests
{
    [Fact]
    public void _01_Without_A_Display_Hook_Should_Be_Unavailable_And_Harmless()
    {
        Assert.SkipWhen(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")),
            "A display is available : the hook connects to it.");

        using var hook = new LinuxHookService((_, _) => { });

        hook.IsAvailable.Should().BeFalse();
        hook.Hwnd.Should().Be(nint.Zero);
        hook.RegisterHotKey(10001, 2, 0x44).Should().BeFalse();
        hook.UnregisterHotKey(10001); // must not throw
    }

    [Fact]
    public void _02_Dispose_Should_Be_Idempotent()
    {
        var hook = new LinuxHookService((_, _) => { });

        hook.Dispose();
        var act = hook.Dispose;

        act.Should().NotThrow();
        hook.RegisterHotKey(10001, 2, 0x44).Should().BeFalse();
    }
}
