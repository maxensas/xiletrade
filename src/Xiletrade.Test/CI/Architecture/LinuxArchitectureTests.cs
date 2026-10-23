using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using System.Runtime.Versioning;

namespace Xiletrade.Test.CI.Architecture;

public class LinuxArchitectureTests
{
    private const string LinuxNamespace = "Xiletrade.Library.Services.Linux";

    private static readonly Assembly Lib = typeof(Library.Services.DataManagerService).Assembly;

    [Fact]
    public void _01_Linux_Implementations_Should_Live_In_Linux_Namespace()
    {
        var result = Types.InAssembly(Lib).That()
            .HaveNameStartingWith("Linux")
            .Should().ResideInNamespace(LinuxNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void _02_Linux_Services_Should_Declare_SupportedOSPlatform()
    {
        var missing = Lib.GetTypes()
            .Where(t => t.Namespace == LinuxNamespace && t.IsClass && !t.IsNested)
            .Where(t => !t.GetCustomAttributes<SupportedOSPlatformAttribute>().Any())
            .Select(t => t.Name);
        missing.Should().BeEmpty();
    }

    [Fact]
    public void _03_Linux_Services_Should_Only_Target_Linux()
    {
        var wrong = Lib.GetTypes()
            .Where(t => t.Namespace == LinuxNamespace && t.IsClass && !t.IsNested)
            .Where(t => t.GetCustomAttributes<SupportedOSPlatformAttribute>().Any(a => a.PlatformName != "linux"))
            .Select(t => t.Name);
        wrong.Should().BeEmpty();
    }

    [Fact]
    public void _04_Linux_Services_Should_Not_Depend_On_Windows_Implementations()
    {
        var result = Types.InAssembly(Lib).That()
            .ResideInNamespace(LinuxNamespace)
            .ShouldNot().HaveDependencyOnAny("Xiletrade.Library.Services.Windows",
                "Xiletrade.Library.Shared.Interop.Windows")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void _05_Windows_Services_Should_Not_Depend_On_Linux_Implementations()
    {
        var result = Types.InAssembly(Lib).That()
            .ResideInNamespace("Xiletrade.Library.Services.Windows")
            .ShouldNot().HaveDependencyOnAny(LinuxNamespace, "Xiletrade.Library.Shared.Interop.Linux")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }
}
