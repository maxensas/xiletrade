using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void InstantiateMainView() => _serviceProvider.GetRequiredService<MainView>();

    public void ShowMainView()
    {
        Action showMainWindow = new(() =>
        {
            try
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.ShowActivated = false;
            }
            catch (Exception)
            {
                //nothing
            }
        });
        DelegateActionToUiThread(showMainWindow);
    }

    public bool IsVisibleMainView()
    {
        return _serviceProvider.GetRequiredService<MainView>().IsVisible;
    }

    public void CloseMainView()
    {
        var win = _serviceProvider.GetRequiredService<MainView>();
        if (win.IsVisible)
        {
            win.Close();
        }
    }

    public void ShowEditorView() => _serviceProvider.GetRequiredService<EditorView>().Show();

    public void ShowConfigView() => _serviceProvider.GetRequiredService<ConfigView>().Show();

    public async Task ShowStartView()
    {
        var service = _serviceProvider.GetRequiredService<IWindowService>();
        await service.CreateDialog<StartView>(new StartViewModel(_serviceProvider)).ConfigureAwait(false);
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data)
    {
        var service = _serviceProvider.GetRequiredService<IWindowService>();
        service.CreateWindow<WhisperListView>(new WhisperViewModel(_serviceProvider, data), false);
    }

    public void ShowPopupView(string imgName)
    {
        PopView Popup = new(imgName); // viewmodel not used.
    }

    public void SetMainHandle(object view)
    {
        if (view is Window win)
        {
            _serviceProvider.GetRequiredService<XiletradeService>().MainHwnd
                = new System.Windows.Interop.WindowInteropHelper(win).Handle;
        }
    }

    public void DelegateActionToUiThread(Action action)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            action();
            return;
        }
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action); //previous DispatcherPriority.Background
    }

    public TResult DelegateFuncToUiThread<TResult>(Func<TResult> func)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            return func();
        }
        return Application.Current.Dispatcher.Invoke(func, DispatcherPriority.Normal);
    }

    // not async
    public Task<TResult> DelegateActionToUiThreadAsync<TResult>(Func<Task<TResult>> asyncFunc)
    {
        return Task.Run(() => DelegateFuncToUiThread(asyncFunc));
    }

    public void ShutDownXiletrade(int code = 0) => Application.Current.Shutdown(code);

    public string GetKeyPressed(EventArgs e)
    {
        string keyPressed = string.Empty;
        if (e is KeyEventArgs keyArg)
        {
            List<Key> modKeyList = new()
            {
                Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl,
                Key.LWin, Key.RWin, Key.LeftAlt, Key.RightAlt
            };

            var key = keyArg.Key switch
            {
                Key.System => keyArg.SystemKey,
                Key.ImeProcessed => keyArg.ImeProcessedKey,
                Key.DeadCharProcessed => keyArg.DeadCharProcessedKey,
                _ => keyArg.Key,
            };

            bool isModKey = modKeyList.Contains(key);

            if (keyArg.IsDown && !modKeyList.ToArray().Contains(key))
            {
                var modifiers = new List<ModifierKeys>();
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Control);
                }

                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Alt);
                }

                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !isModKey)
                {
                    modifiers.Add(ModifierKeys.Shift);
                }

                string modifStr = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys)).ConvertToString(Keyboard.Modifiers);
                string hotKey = modifiers.Count is 0 ? string.Format("{0}", key)
                    : string.Format("{0}+{1}", modifStr, key);

                if (VerifyHotKey(hotKey))
                {
                    if (hotKey.Length is 2 && hotKey.StartsWith('D')) // D0 to D9
                    {
                        hotKey = hotKey.Replace("D", string.Empty);
                    }
                    keyPressed = hotKey;
                }
            }
            keyArg.Handled = true;
        }
        return keyPressed;
    }

    private bool VerifyHotKey(string hotKeyText)
    {
        if (hotKeyText.EndsWith('+')) // cannot set '+' as hotkey : ok for OemPlus & NumpadPlus
        {
            return false;
        }
        try
        {
            var kc = _serviceProvider.GetRequiredService<IKeysConverter>();
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
        static bool GetMod(string text, ModifierKeys modkey)
        {
            var mkc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys));
            return text.ToLowerInvariant().Contains(mkc.ConvertToString(modkey).ToLowerInvariant(), StringComparison.Ordinal);
        }

        int mod = MOD_NONE;
        if (GetMod(modifier, ModifierKeys.Control))
        {
            mod |= MOD_CONTROL;
        }
        if (GetMod(modifier, ModifierKeys.Alt))
        {
            mod |= MOD_ALT;
        }
        if (GetMod(modifier, ModifierKeys.Shift))
        {
            mod |= MOD_SHIFT;
        }
        return mod;
    }

    public string GetModifierText(int modifier)
    {
        string returnVal = string.Empty;
        var modifiers = Enum.Parse<ModifierKeys>(modifier.ToString());

        if (modifiers.HasFlag(ModifierKeys.Control) || modifiers.HasFlag(ModifierKeys.Alt) || modifiers.HasFlag(ModifierKeys.Shift))
        {
            returnVal += System.ComponentModel.TypeDescriptor.GetConverter(typeof(ModifierKeys)).ConvertToString(modifiers) + "+";
        }

        return returnVal;
    }

    public void ClearKeyboardFocus() => Keyboard.ClearFocus();

    public void ShowRegexView() => _serviceProvider.GetRequiredService<RegexView>().Show();

    //.ShowDialog();
    public void ShowUpdateView(GitHubRelease release)
    {
        Action showUpdateWindow = new(() =>
        {
            var view = _serviceProvider.GetRequiredService<UpdateView>();
            view.DataContext = new UpdateViewModel(release, _serviceProvider);
            view.ShowDialog();
        });
        DelegateActionToUiThread(showUpdateWindow);
    }
}
