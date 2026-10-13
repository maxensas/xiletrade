using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Xiletrade.Library.Models.Serialization.SourceGeneration;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Test.CI.Architecture;

public class ArchitectureTests
{
    private const string FeatureNamespace = "Xiletrade.Library.Models.Application.Feature";

    private static readonly Assembly Lib = typeof(Library.Services.DataManagerService).Assembly;

    protected static void AssertSuccess(NetArchTest.Rules.TestResult r) =>
        r.IsSuccessful.Should().BeTrue(string.Join(", ", r.FailingTypeNames ?? []));

    private static bool IsCompilerGenerated(Type type)
    {
        // Covers <>c, <>c__DisplayClass..., async state machines, etc.,
        // including when nested within a generated type.
        for (var t = type; t is not null; t = t.DeclaringType)
        {
            if (t.Name.StartsWith('<') || t.IsDefined(typeof(CompilerGeneratedAttribute), false))
                return true;
        }
        return false;
    }

    private static bool IsPlatformSpecific(Type type)
    {
        // A nested type inherits the platform of its enclosing type.
        for (var t = type; t is not null; t = t.DeclaringType)
        {
            if (t.GetCustomAttributes<SupportedOSPlatformAttribute>(inherit: false).Any())
                return true;
        }
        return false;
    }

    [Fact]
    public void _01_Library_Should_Not_Depend_On_UI_Frameworks()
    {
        var result = Types.InAssembly(Lib).ShouldNot()
            .HaveDependencyOnAny("System.Windows", "Avalonia", "System.Windows.Forms")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void _02_Library_Should_Not_Reference_UI_Assemblies()
    {
        string[] forbidden = ["PresentationFramework", "PresentationCore", "WindowsBase",
                          "System.Windows.Forms", "Avalonia"];
        var refs = Lib.GetReferencedAssemblies().Select(a => a.Name!);
        refs.Where(n => forbidden.Any(n.StartsWith)).Should().BeEmpty();
    }

    [Fact]
    public void _03_Windows_Implementations_Should_Live_In_Windows_Namespace()
    {
        var result = Types.InAssembly(Lib).That()
            .HaveNameStartingWith("Windows")
            .Should().ResideInNamespace("Xiletrade.Library.Services.Windows")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void _04_Windows_Services_Should_Declare_SupportedOSPlatform()
    {
        var missing = Lib.GetTypes()
            .Where(t => t.Namespace == "Xiletrade.Library.Services.Windows" && t.IsClass && !t.IsNested)
            .Where(t => !t.GetCustomAttributes<SupportedOSPlatformAttribute>().Any())
            .Select(t => t.Name);
        missing.Should().BeEmpty();
    }

    [Fact]
    public void _05_PInvoke_Should_Only_Live_In_Interop_Or_Platform_Specific_Types()
    {
        const BindingFlags All = BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.Public | BindingFlags.NonPublic;

        var offenders = Lib.GetTypes()
            .Where(t => t.Namespace?.StartsWith("Xiletrade.Library.Shared.Interop") != true)
            .Where(t => !IsPlatformSpecific(t))
            .Where(t => t.GetMethods(All).Any(m => (m.Attributes & MethodAttributes.PinvokeImpl) != 0))
            .Select(t => t.FullName);

        offenders.Should().BeEmpty();
    }

    [Fact]
    public void _06_Contract_Types_Should_Be_Registered_In_SourceGenerationContext()
    {
        var ctx = SourceGenerationContext.Default;
        var missing = Lib.GetTypes()
            .Where(t => t.IsClass && t.IsPublic && t.Namespace?.Contains(".Contract") == true)
            .Where(t => ctx.GetTypeInfo(t) is null)
            .Select(t => t.FullName);
        missing.Should().BeEmpty();
    }

    [Fact]
    public void _07_No_Public_Mutable_Static_Fields()
    {
        const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        var offenders = Lib.GetTypes()
            .Where(t => !IsCompilerGenerated(t))
            .SelectMany(t => t.GetFields(PublicStatic))
            .Where(f => !f.IsLiteral && !f.IsInitOnly)
            .Where(f => !f.Name.StartsWith('<') && !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Select(f => $"{f.DeclaringType!.FullName}.{f.Name}");

        offenders.Should().BeEmpty();
    }

    [Fact]
    public void _08_Feature_Classes_Should_Be_Named_With_Feature_Suffix()
    {
        var result = Types.InAssembly(Lib)
            .That().ResideInNamespace(FeatureNamespace)
            .And().AreClasses()
            .Should().HaveNameEndingWith("Feature")
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void _09_Feature_Classes_Should_Inherit_From_BaseFeature()
    {
        var result = Types.InAssembly(Lib)
            .That().ResideInNamespace(FeatureNamespace)
            .And().AreClasses()
            .And().DoNotHaveName("BaseFeature")
            .Should().Inherit(Lib.GetType($"{FeatureNamespace}.BaseFeature")!)
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void _10_ViewModel_Classes_Should_Inherit_From_ViewModelBase()
    {
        var result = Types.InAssembly(Lib)
            .That().HaveNameEndingWith("ViewModel")
            .And().AreClasses()
            .Should().Inherit(typeof(ViewModelBase))
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void _11_ViewModels_Should_Only_See_Views_Through_Interfaces()
    {
        // Concrete views reside in UI projects; the library only sees IxxxView.
        var result = Types.InAssembly(Lib).That()
            .ResideInNamespace("Xiletrade.Library.ViewModels")
            .Should().NotHaveDependencyOn("Xiletrade.UI.WPF")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    // Not the case with Xiletrade
    /*
    [Fact]
    public void _12_Models_Should_Not_Depend_On_ViewModels_Or_Services()
    {
        var result = Types.InAssembly(Lib).That()
            .ResideInNamespace("Xiletrade.Library.Models")
            .ShouldNot().HaveDependencyOnAny(
                "Xiletrade.Library.ViewModels", "Xiletrade.Library.Services")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }
    */
}