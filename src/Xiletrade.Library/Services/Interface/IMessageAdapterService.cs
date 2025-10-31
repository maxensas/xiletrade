using System.Threading.Tasks;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Interface;

public interface IMessageAdapterService
{
    /// <summary>
    /// Show a message box without waiting for a result.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="caption"></param>
    /// <param name="status"></param>
    public void Show(string message, string caption, MessageStatus status);

    /// <summary>
    /// Show a message box and wait for a result.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="caption"></param>
    /// <param name="status"></param>
    /// <param name="yesNo"></param>
    /// <returns>Return True if OK or YES is pressed, otherwise False</returns>
    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false);

    /// <summary>
    /// Show a message box and wait for a result.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="caption"></param>
    /// <param name="status"></param>
    /// <param name="yesNo"></param>
    /// <returns>Return True if OK or YES is pressed, otherwise False</returns>
    public Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false);
}
