namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides access to the native message window.
/// </summary>
public interface IHookService
{
    nint Hwnd { get; }
}