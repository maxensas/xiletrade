using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.UI.Avalonia.Views;

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

    // unusable
    public void CreateDialog<T>(object dataContext) where T : IViewBase
    {
        var type = typeof(T);
        if (Activator.CreateInstance(type, dataContext) is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        bool isClosed = false;
        window.Closed += (_, __) => { isClosed = true; };

        window.Show();

        // Wait loop that keeps the Dispatcher UI running
        while (!isClosed)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
    }

    public async Task CreateDialogAsync<T>(object dataContext) where T : IViewBase
    {
        var type = typeof(T);

        var instance = Activator.CreateInstance(type, dataContext)
            ?? throw new InvalidOperationException($"Cannot create instance of {type.Name}");

        if (instance is not Window window)
            throw new InvalidOperationException($"Type {type.Name} must be a Window.");

        //var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var owner = _serviceProvider.GetRequiredService<MainView>();
        if (owner == null)
            throw new InvalidOperationException("No main window found as owner.");

        // Display and wait for closing
        await window.ShowDialog(owner);
    }
}
