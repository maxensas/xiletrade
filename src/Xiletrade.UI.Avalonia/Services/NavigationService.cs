using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia.Services;

public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void ClearKeyboardFocus()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            window?.Focus();
        }
    }

    public void CloseMainView()
    {
        var win = _serviceProvider.GetRequiredService<MainView>();
        if (win.IsVisible)
        {
            win.Close();
        }
    }

    public void DelegateActionToUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }
        Dispatcher.UIThread.Post(action, DispatcherPriority.Normal); //previous DispatcherPriority.Background
    }

    public TResult DelegateFuncToUiThread<TResult>(Func<TResult> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return func();
        }
        return Dispatcher.UIThread.Invoke(func, DispatcherPriority.Normal);
    }

    // only en for now
    public string GetKeyPressed(EventArgs ev)
    {
        string keyPressed = string.Empty;

        if (ev is null || ev is not KeyEventArgs)
            return keyPressed;
        var e = ev as KeyEventArgs;
        var modKeyList = new List<Key>
        {
            Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl,
            Key.LeftAlt, Key.RightAlt, Key.LWin, Key.RWin
        };

        var key = e.Key;

        bool isModKey = modKeyList.Contains(key);

        if (e.KeyModifiers != KeyModifiers.None && isModKey)
        {
            // Ignore pure modifier keys
            return string.Empty;
        }

        if (!isModKey)
        {
            var modifiers = new List<string>();

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                modifiers.Add("Ctrl");

            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                modifiers.Add("Alt");

            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                modifiers.Add("Shift");

            string keyName = key.ToString();

            // Normalize D0-D9 keys (e.g., D1 -> 1)
            if (keyName.StartsWith("D") && keyName.Length == 2 && char.IsDigit(keyName[1]))
                keyName = keyName[1].ToString();

            string modifStr = string.Join("+", modifiers);

            string hotKey = modifiers.Count == 0
                ? keyName
                : $"{modifStr}+{keyName}";

            if (VerifyHotKey(hotKey))
            {
                keyPressed = hotKey;
            }
        }

        e.Handled = true;
        return keyPressed;
    }

    private bool VerifyHotKey(string hotKeyText)
    {
        if (hotKeyText.EndsWith('+')) // cannot set '+' as hotkey : ok for OemPlus & NumpadPlus
        {
            return false;
        }
        var kc = _serviceProvider.GetRequiredService<IKeysConverter>();
        try
        {
            var returnKey = (int)kc.ConvertFromInvariantString(hotKeyText);
            return true;
        }
        catch // exception not used
        {
            return false;
        }
    }

    private static readonly int MOD_NONE = 0x0;    // No modifier
    private static readonly int MOD_ALT = 0x1;     // If bit 0 is set, Alt is pressed
    private static readonly int MOD_CONTROL = 0x2; // If bit 1 is set, Ctrl is pressed
    private static readonly int MOD_SHIFT = 0x4;   // If bit 2 is set, Shift is pressed 
    //private static readonly int MOD_WIN = 0x8;   // If bit 3 is set, Win is pressed

    public int GetModifierCode(string modifier)
    {
        static bool GetMod(string text, KeyModifiers modkey)
        {
            var mkc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(KeyModifiers));
            return text.ToLowerInvariant().Contains(mkc.ConvertToString(modkey).ToLowerInvariant(), StringComparison.Ordinal);
        }

        int mod = MOD_NONE;
        if (GetMod(modifier, KeyModifiers.Control))
        {
            mod |= MOD_CONTROL;
        }
        if (GetMod(modifier, KeyModifiers.Alt))
        {
            mod |= MOD_ALT;
        }
        if (GetMod(modifier, KeyModifiers.Shift))
        {
            mod |= MOD_SHIFT;
        }
        return mod;
    }

    public string GetModifierText(int modifier)
    {
        string returnVal = string.Empty;
        var modifiers = Enum.Parse<KeyModifiers>(modifier.ToString());

        if (modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Alt) || modifiers.HasFlag(KeyModifiers.Shift))
        {
            returnVal += System.ComponentModel.TypeDescriptor.GetConverter(typeof(KeyModifiers)).ConvertToString(modifiers) + "+";
        }

        return returnVal;
    }

    public void InstantiateMainView() => _serviceProvider.GetRequiredService<MainView>();

    public bool IsVisibleMainView() => _serviceProvider.GetRequiredService<MainView>().IsVisible;

    public void SetMainHandle(object view)
    {
        if (view is Window win)
        {
            var platformImpl = (win.PlatformImpl as IPlatformHandle);
            IntPtr hwnd = platformImpl?.Handle ?? IntPtr.Zero;
            _serviceProvider.GetRequiredService<XiletradeService>().MainHwnd = hwnd;
        }

    }

    public void ShowConfigView() => _serviceProvider.GetRequiredService<PopView>().Show();

    public void ShowEditorView() => _serviceProvider.GetRequiredService<EditorView>().Show();

    public void ShowMainView()
    {
        Action showMainWindow = new(() =>
        {
            try
            {
                var win = _serviceProvider.GetRequiredService<MainView>();
                win.WindowStartupLocation = WindowStartupLocation.Manual;
                win.Show();
            }
            catch (Exception)
            {
                //nothing
            }
        });
        DelegateActionToUiThread(showMainWindow);
    }

    public void ShowPopupView(string imgName)
    {
        PopView Popup = new(imgName); // viewmodel not used.
    }

    public void ShowRegexView() => _serviceProvider.GetRequiredService<RegexView>().Show();

    public void ShowStartView()
    {
        var service = _serviceProvider.GetRequiredService<WindowService>();
        //service.CreateDialog<StartView>(new StartViewModel(_serviceProvider));
        //TODO move everything to async
        //service.CreateDialogAsync<StartView>(new StartViewModel(_serviceProvider));
    }

    public void ShowUpdateView(GitHubRelease release)
    {
        Action showUpdateWindow = new(() =>
        {
            var view = _serviceProvider.GetRequiredService<UpdateView>();
            view.DataContext = new UpdateViewModel(release, _serviceProvider);
            view.ShowDialog(null);
        });
        DelegateActionToUiThread(showUpdateWindow);
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data)
    {
        var service = _serviceProvider.GetRequiredService<IWindowService>();
        service.CreateWindow<WhisperListView>(new WhisperViewModel(_serviceProvider, data), false);
    }

    public void ShutDownXiletrade(int code = 0)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(code);
        }
    }

    public void UpdateControlValue(object obj, double value = 0)
    {
        //throw new NotImplementedException();
    }
}
