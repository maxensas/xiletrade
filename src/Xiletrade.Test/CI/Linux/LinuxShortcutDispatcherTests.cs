using System.Runtime.Versioning;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Test.CI.Linux;

[SupportedOSPlatform("linux")]
public class LinuxShortcutDispatcherTests
{
    private readonly Mock<IMessageAdapterService> _message = new();
    private readonly Mock<IUIService> _ui = new();
    private readonly List<Action> _posted = [];
    private readonly LinuxShortcutDispatcher _dispatcher;

    public LinuxShortcutDispatcherTests()
    {
        // The UI thread is simulated : the posted actions are run by the test.
        _ui.Setup(u => u.Invoke(It.IsAny<Action>())).Callback<Action>(_posted.Add);

        // Concrete services of the library, whose state is not used before a feature is launched.
        var dm = (DataManagerService)RuntimeHelpers.GetUninitializedObject(typeof(DataManagerService));
        var features = (FeatureProvider)RuntimeHelpers.GetUninitializedObject(typeof(FeatureProvider));

        _dispatcher = new LinuxShortcutDispatcher(Mock.Of<ILogger<LinuxShortcutDispatcher>>(),
            _message.Object, dm, features, _ui.Object);
    }

    [Fact]
    public void _01_Other_Messages_Should_Be_Ignored()
    {
        _dispatcher.HandleMessage(0x0001, IInputService.SHIFTHOTKEYID);

        _posted.Should().BeEmpty();
    }

    [Fact]
    public void _02_Hotkey_Should_Be_Launched_On_The_Ui_Thread()
    {
        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);

        _posted.Should().HaveCount(1);
    }

    [Fact]
    public void _03_Hotkey_Pressed_While_Another_Is_Pending_Should_Be_Ignored()
    {
        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);
        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID + 1);

        _posted.Should().HaveCount(1);
    }

    [Fact]
    public void _04_Failure_Should_Be_Reported_And_Should_Not_Block_The_Next_Hotkey()
    {
        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);

        // the configuration of the uninitialized data manager is missing : the feature fails
        _posted[0]();

        _message.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), MessageStatus.Error), Times.Once);

        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);
        _posted.Should().HaveCount(2);
    }

    [Fact]
    public void _05_Failure_To_Reach_The_Ui_Thread_Should_Not_Block_The_Next_Hotkey()
    {
        int calls = 0;
        _ui.Setup(u => u.Invoke(It.IsAny<Action>())).Callback<Action>(_ =>
        {
            calls++;
            if (calls is 1)
            {
                throw new InvalidOperationException("no UI thread");
            }
        });

        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);
        _dispatcher.HandleMessage(LinuxHookService.HotkeyMessage, IInputService.SHIFTHOTKEYID);

        calls.Should().Be(2);
    }
}
