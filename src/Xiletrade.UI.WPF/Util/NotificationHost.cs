using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.UI.WPF.Util;

/// <summary>
/// Shows independent desktop toasts on the screen of the game, stacked by placement (bottom right above the Windows taskbar by default).
/// </summary>
/// <remarks>
/// The backgrounds take the opacity set by the user in the options.
/// The toasts with a content (live searches) are scaled with the screen : same proportions whatever its size.
/// Hidden while another application than the game or Xiletrade is in the foreground (as the shortcuts, <see cref="GameFocusMessage"/>).
/// The toasts are tool windows not activated by a click : not listed by Alt+Tab, the game keeps the focus.
/// </remarks>
public sealed class NotificationHost : INotificationHost
{
    private readonly IUIService _ui;
    private readonly DataManagerService _dm;
    private readonly Action<string> _dismiss;

    // Insertion order = stacking order (oldest first), guaranteed unlike Dictionary.
    private readonly OrderedDictionary<string, (Window Window, NotificationPlacement Placement)> _windows = new(StringComparer.Ordinal);
    private readonly HashSet<string> _closing = new(StringComparer.Ordinal);

    private readonly ToastOptions _toast = new();
    private readonly Duration _fadeDuration;

    // frozen brushes with the opacity of the options (key of the theme or color of a status), built again when it changes
    private readonly Dictionary<string, Brush> _brushes = new(StringComparer.Ordinal);
    private double _brushOpacity = double.NaN;

    // a window is being closed by the host : not a closing to tell to the notification service
    private bool _hostClosing;

    // the game or Xiletrade has the focus : the toasts are displayed
    private bool _hasGameFocus = true;

    private bool _disposed;

    /// <summary>
    /// Height kept free at the bottom of the screen by a window glued to it (buttons of the live searches) :
    /// the toasts placed at the bottom right are stacked above it.
    /// </summary>
    public static double ReservedBottom { get; set; }

    public NotificationHost(IUIService ui, DataManagerService dm, Action<string> dismiss)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _dm = dm ?? throw new ArgumentNullException(nameof(dm));
        _dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));

        _fadeDuration = new(TimeSpan.FromMilliseconds(_toast.FadeDuration));

        WeakReferenceMessenger.Default.Register<NotificationHost, GameFocusMessage>(this,
            static (host, focus) => host.OnGameFocus(focus.HasFocus));
    }

    // hidden with the shortcuts, displayed again over the game (Topmost set again : at the top of the topmost windows)
    private void OnGameFocus(bool hasFocus) => _ui.Invoke(() =>
    {
        if (_hasGameFocus == hasFocus)
            return;

        _hasGameFocus = hasFocus;
        foreach (var (window, _) in _windows.Values)
        {
            if (!hasFocus)
            {
                window.Hide();
                continue;
            }
            window.Show();
            window.Topmost = false;
            window.Topmost = true;
        }
        if (hasFocus)
        {
            // after the buttons of the live searches are displayed again : their height is kept free
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(Reposition));
        }
    });

    public void OnAdded(object sender, ToastNotification message) => _ui.Invoke(() =>
    {
        if (_windows.Remove(message.Id, out var old))
        {
            _closing.Remove(message.Id);
            CloseWindow(old.Window);
        }

        var opacity = _dm.Opacity;
        if (opacity != _brushOpacity)
        {
            _brushes.Clear();
            _brushOpacity = opacity;
        }

        var panel = new DockPanel
        {
            Margin = new Thickness(message.Content is null ? _toast.Padding : _toast.ContentPadding),
            Background = Brushes.Transparent
        };

        if (message.Content is not null)
        {
            // view model displayed by its template (application resources) : closed by its own buttons
            panel.Children.Add(new ContentControl { Content = message.Content, Focusable = false });
        }
        else
        {
            panel.Cursor = System.Windows.Input.Cursors.Hand;
            panel.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                _dismiss(message.Id);
            };
            panel.Children.Add(CreateText(message));
        }

        var border = new Border
        {
            Background = GetStatusBrush(message.Status, opacity),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = panel
        };
        if (message.Content is not null)
        {
            // same background and border as the window of Xiletrade
            border.Background = GetThemeBrush("WindowBackgroundBrush", opacity) ?? border.Background;
            border.BorderBrush = GetThemeBrush("WindowBorderBrush", opacity);
            border.BorderThickness = new Thickness(2);
        }
        if (message.IsPinned)
        {
            border.BorderBrush = GetThemeBrush("PinnedBorderBrush", opacity);
            border.BorderThickness = new Thickness(2);
        }

        var width = _toast.Width;
        if (message.Content is not null)
        {
            // same proportions of the screen as on the reference screen
            var scale = GameScreen.Get().Scale;
            if (scale != 1)
            {
                var transform = new ScaleTransform(scale, scale);
                transform.Freeze();
                border.LayoutTransform = transform;
                width *= scale;
            }
        }

        var window = new Window
        {
            Title = message.Title,
            Content = border,
            Width = width,
            SizeToContent = SizeToContent.Height,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Topmost = true,
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            AllowsTransparency = true,
            WindowStartupLocation = WindowStartupLocation.Manual,
            ShowActivated = false,
            Opacity = 0
        };

        window.Closed += (_, _) =>
        {
            var isCurrent = _windows.TryGetValue(message.Id, out var current) && ReferenceEquals(current.Window, window);
            if (isCurrent)
            {
                _windows.Remove(message.Id);
                _closing.Remove(message.Id);
            }

            window.Content = null;
            Reposition();

            // closed by the system, not by the service : removed from it too (a pinned toast frees its place)
            if (isCurrent && !_hostClosing && !_disposed && !window.Dispatcher.HasShutdownStarted)
            {
                _dismiss(message.Id);
            }
        };

        window.SourceInitialized += OnSourceInitialized; // before it is visible
        _windows[message.Id] = (window, message.Placement);

        window.Show(); // synchronous layout: ActualWidth/ActualHeight are valid right after
        Reposition();  // placed before the fade-in starts, so nothing is visible at a wrong position
        if (!_hasGameFocus)
        {
            window.Hide(); // still transparent (fade-in not started) : displayed when the game has the focus again
        }

        window.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, _fadeDuration));
        if (message.PlaySound)
        {
            System.Media.SystemSounds.Hand.Play();
        }
    });

    // not listed by Alt+Tab, not activated by a click
    private static void OnSourceInitialized(object sender, EventArgs e)
        => Native.AddExtendedStyle(new WindowInteropHelper((Window)sender).Handle, Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);

    // title and message of a toast without content
    private StackPanel CreateText(ToastNotification message)
    {
        var text = new StackPanel();

        text.Children.Add(new TextBlock
        {
            Text = message.Title,
            FontWeight = FontWeights.SemiBold,
            FontSize = _toast.TitleFontSize,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        });

        text.Children.Add(new TextBlock
        {
            Text = message.Message,
            FontSize = _toast.MessageFontSize,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = Brushes.White
        });

        return text;
    }

    public void OnRemoved(string id, bool fade) => _ui.Invoke(() =>
    {
        if (!_windows.TryGetValue(id, out var toast))
            return;

        var window = toast.Window;
        if (!fade)
        {
            // at once : the Closed handler removes the entry and repositions the remaining toasts
            CloseWindow(window);
            return;
        }

        // Already fading out (e.g. click then timer expiry): ignore.
        if (!_closing.Add(id))
            return;

        // The toast stays in the stack while fading, so the others don't jump early.
        var animation = new DoubleAnimation(window.Opacity, 0, _fadeDuration);

        animation.Completed += (_, _) =>
        {
            // Skip if the window was replaced or closed in the meantime.
            if (!_windows.TryGetValue(id, out var current) || !ReferenceEquals(current.Window, window))
                return;

            // Closed handler removes the entry and repositions the remaining toasts.
            CloseWindow(window);
        };

        window.BeginAnimation(UIElement.OpacityProperty, animation);
    });

    private void CloseWindow(Window window)
    {
        _hostClosing = true;
        try
        {
            window.Close();
        }
        finally
        {
            _hostClosing = false;
        }
    }

    private void Reposition()
    {
        if (_windows.Count is 0)
            return;

        var screen = GameScreen.Get();
        var area = screen.WorkArea;
        var bounds = screen.Bounds;
        var minTop = area.Top + _toast.Margin;
        var maxBottom = area.Bottom - _toast.Margin;

        // one stack by placement : bottom of the stacks growing upwards, top of the stack growing downwards
        var bottom = Math.Min(area.Bottom, bounds.Bottom - ReservedBottom) - _toast.Margin;
        var quarter = bounds.Bottom - bounds.Height / 4;
        var top = minTop;

        // Newest first = nearest to the start of its stack.
        foreach (var (window, placement) in _windows.Values.Reverse().ToArray())
        {
            if (!window.IsVisible)
                continue;

            window.Left = area.Right - window.ActualWidth - _toast.Margin;
            switch (placement)
            {
                case NotificationPlacement.TopRight:
                    window.Top = Math.Min(maxBottom - window.ActualHeight, top);
                    top += window.ActualHeight + _toast.Gap;
                    break;
                case NotificationPlacement.LowerQuarterRight:
                    quarter -= window.ActualHeight;
                    window.Top = Math.Max(minTop, quarter);
                    quarter -= _toast.Gap;
                    break;
                default:
                    bottom -= window.ActualHeight;
                    window.Top = Math.Max(minTop, bottom);
                    bottom -= _toast.Gap;
                    break;
            }
        }
    }

    // brush of the theme (application resources) with the opacity of the options
    private Brush GetThemeBrush(string key, double opacity)
    {
        if (_brushes.TryGetValue(key, out var cached))
            return cached;

        if (Application.Current.TryFindResource(key) is not Brush theme)
            return null;

        var brush = theme.CloneCurrentValue();
        brush.Opacity = opacity;
        brush.Freeze();
        _brushes[key] = brush;
        return brush;
    }

    private Brush GetStatusBrush(NotificationStatus status, double opacity)
    {
        var color = _toast.GetStatusColor(status);
        if (_brushes.TryGetValue(color, out var cached))
            return cached;

        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
        {
            Opacity = opacity
        };
        brush.Freeze();
        _brushes[color] = brush;
        return brush;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        WeakReferenceMessenger.Default.UnregisterAll(this);

        _ui.Invoke(() =>
        {
            foreach (var (window, _) in _windows.Values.ToArray())
                CloseWindow(window);

            _windows.Clear();
            _closing.Clear();
        });
    }
}