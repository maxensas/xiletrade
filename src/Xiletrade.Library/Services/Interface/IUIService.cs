using System;
using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

public interface IUIService
{
    nint MainHwnd { get; set; }

    /// <summary>
    /// Executes the action on the UI thread without waiting for completion.
    /// </summary>
    void DelegateActionToUiThread(Action action);

    /// <summary>
    /// Executes the function on the UI thread and waits for the result.
    /// </summary>
    TResult DelegateFuncToUiThread<TResult>(Func<TResult> func);

    /// <summary>
    /// Executes an asynchronous function on the UI thread.
    /// </summary>
    Task<TResult> DelegateActionToUiThreadAsync<TResult>(Func<Task<TResult>> asyncFunc);

    void ShutDownXiletrade(int code = 0);
}
