using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.Update;
using Xiletrade.Library.ViewModels.Whisper;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Provides window management services for the Xiletrade application, including showing or closing views and handling keyboard input.
/// </summary>
/// <remarks>
///  WPF UI Framework implementation of the INavigationService interface. 
/// </remarks>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _sp;
    private readonly IKeysConverter _keyConv;
    private readonly IMessageAdapterService _message;
    private readonly IUpdateDownloader _updater;
    private readonly LocalizationService _localization;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;
    private readonly UIService _ui;
    
    private IViewBase ConfigView => _sp.GetRequiredService<IConfigView>();
    private IViewBase UpdateView => _sp.GetRequiredService<IUpdateView>();
    private IViewBase RegexView => _sp.GetRequiredService<IRegexView>();
    private IViewBase EditorView => _sp.GetRequiredService<IEditorView>();

    public NavigationService(IServiceProvider sp, ILogger<NavigationService> logger,
        IKeysConverter keyConv, IMessageAdapterService message, 
        IUpdateDownloader updater, LocalizationService localization,
        ClipboardService clipboard, DataManagerService dm, UIService ui)
    {
        _sp = sp;
        _keyConv = keyConv;
        _message = message;
        _updater = updater;
        _localization = localization;
        _clipboard = clipboard;
        _dm = dm;
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void ShowMainView()
    {
        Action showMainView = new(() =>
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
        _ui.DelegateActionToUiThread(showMainView);
    }

    public bool IsVisibleMainView()
    {
        return Application.Current.MainWindow is not null 
            && Application.Current.MainWindow.IsVisible;
    }

    public void CloseMainView()
    {
        if (Application.Current.MainWindow is not null 
            && Application.Current.MainWindow.IsVisible)
        {
            Application.Current.MainWindow.Close();
        }
    }

    public void ShowEditorView() => EditorView.Show();

    public void ShowConfigView() => ConfigView.Show();

    public async Task ShowStartView() => await CreateDialog<StartView>
        (new StartViewModel(_dm, _localization)).ConfigureAwait(false);

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data) 
        => CreateWindow<WhisperListView>(new WhisperViewModel(_dm, _clipboard, data), false);

    public void ShowPopupView(string imgName)
    {
        PopView Popup = new(imgName); // viewmodel not used.
    }

    public void SetMainHandle(object view)
    {
        if (view is not Window win)
        {
            throw new ArgumentException("The provided view must be a WPF Window.", nameof(view));
        }
        _ui.MainHwnd = new System.Windows.Interop.WindowInteropHelper(win).Handle;
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
            var returnKey = (int)_keyConv.ConvertFromInvariantString(hotKeyText);
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

    public void ShowRegexView() => RegexView.Show();

    //.ShowDialog();
    public void ShowUpdateView(GitHubRelease release)
    {
        Action showUpdateWindow = new(() =>
        {
            var view = UpdateView;
            view.DataContext = new UpdateViewModel(_updater, _message, _ui, release);
            view.ShowDialog();
        });
        _ui.DelegateActionToUiThread(showUpdateWindow);
    }

    private static void CreateWindow<T>(object dataContext, bool show) where T : IViewBase, new()
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;

        if (show)
            window.Show();
    }

    private static Task CreateDialog<T>(object dataContext) where T : IViewBase, new()
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;
        window.ShowDialog();
        return Task.CompletedTask;
    }
}
