namespace Xiletrade.Library.Services.Interface;

/// <summary>Service responsible for hotkey registrations and global input interactions.</summary>
public interface IInputService
{
    const int SHIFTHOTKEYID = 10001;

    void EnableHotkeys();
    void DisableHotkeys();
}
