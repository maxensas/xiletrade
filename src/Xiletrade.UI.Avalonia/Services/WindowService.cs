using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.Avalonia.Services;

public class WindowService(IServiceProvider serviceProvider) : IWindowService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void CreateWindow<T>(object dataContext, bool show) where T : IViewBase, new()
    {
        var window = GetNewWindow<T>(dataContext);
        if (show)
            window.Show();
    }

    public Task CreateDialog<T>(object dataContext) where T : IViewBase, new()
    {
        var window = GetNewWindow<T>(dataContext);
        var tcs = new TaskCompletionSource<T>();
        window.Closed += (_, __) =>
        {
            if (window.DataContext is T result)
                tcs.TrySetResult(result);
            else
                tcs.TrySetResult(default);
        };
        window.Show();

        return tcs.Task;
    }

    private static Window GetNewWindow<T>(object dataContext) where T : IViewBase, new()
    {
        var instance = new T();
        if (instance is not Window window)
            throw new InvalidOperationException("Type must be a Window.");

        window.DataContext = dataContext;
        return window;
    }
}
