using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xiletrade.Library.Services;

public sealed class UIService
{
    public static SynchronizationContext UiThreadContext { get; private set; }

    public nint MainHwnd { get; set; }

    public UIService(ILogger<UIService> logger)
    {
        UiThreadContext = SynchronizationContext.Current ?? 
            throw new InvalidOperationException("UIService must be initialized from the UI thread.");

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    /// <summary>
    /// Executes the action on the UI thread without waiting for completion.
    /// </summary>
    public void DelegateActionToUiThread(Action action)
    {
        if (UiThreadContext is null)
        {
            return;
        }

        if (SynchronizationContext.Current == UiThreadContext)
        {
            action();
            return;
        }

        UiThreadContext.Post(_ => action(), null);
    }

    /// <summary>
    /// Executes the function on the UI thread and waits for the result.
    /// </summary>
    public TResult DelegateFuncToUiThread<TResult>(Func<TResult> func)
    {
        if (UiThreadContext is null)
        {
            return func();
        }

        if (SynchronizationContext.Current == UiThreadContext)
        {
            return func();
        }

        TResult result = default;
        Exception exception = null;

        UiThreadContext.Send(_ =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }, null);

        if (exception is not null)
        {
            throw exception;
        }

        return result!;
    }

    /// <summary>
    /// Executes an asynchronous function on the UI thread.
    /// </summary>
    public Task<TResult> DelegateActionToUiThreadAsync<TResult>(
        Func<Task<TResult>> asyncFunc)
    {
        if (UiThreadContext is null)
        {
            return asyncFunc();
        }

        if (SynchronizationContext.Current == UiThreadContext)
        {
            return asyncFunc();
        }

        var tcs = new TaskCompletionSource<TResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        UiThreadContext.Post(async _ =>
        {
            try
            {
                var result = await asyncFunc();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    public void ShutDownXiletrade(int code = 0) => Environment.Exit(code);
}
