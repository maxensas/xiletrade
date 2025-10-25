using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.Avalonia.Services;

public class WindowService(IServiceProvider serviceProvider) : IWindowService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void CreateWindow<T>(object dataContext, bool show) where T : IViewBase
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;

        if (show)
            window.Show();
    }

    public Task CreateDialog<T>(object dataContext) where T : IViewBase
    {
        var type = typeof(T);

        var instance = Activator.CreateInstance(type, dataContext)
            ?? throw new InvalidOperationException($"Cannot create instance of {type.Name}");

        if (instance is not Window window)
            throw new InvalidOperationException($"Type {type.Name} must be a Window.");

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
}
