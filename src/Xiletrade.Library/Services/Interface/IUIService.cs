using System;
using System.Threading.Tasks;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides UI thread operations and application lifecycle management.
/// </summary>
public interface IUIService
{
    /// <summary>
    /// Executes the action on the UI thread without waiting for completion.
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// Executes a function on the UI thread and returns its result.
    /// </summary>
    TResult Invoke<TResult>(Func<TResult> func);

    /// <summary>
    /// Executes an asynchronous function on the UI thread and returns its result.
    /// </summary>
    Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> asyncFunc);
}
