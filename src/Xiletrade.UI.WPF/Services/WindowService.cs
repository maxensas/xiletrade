using System;
using System.Windows;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

internal class WindowService : IWindowService
{
    public void CreateWindow<T>(object dataContext, bool show) where T : IViewBase
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;

        if (show)
            window.Show();
    }

    public void CreateDialog<T>(object dataContext) where T : IViewBase
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;
        window.ShowDialog();
    }
}
