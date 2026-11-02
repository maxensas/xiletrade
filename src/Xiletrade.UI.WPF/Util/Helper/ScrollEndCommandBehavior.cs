using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.WPF.Util.Helper;

/// <summary>
/// Executes <see cref="Command"/> when the list (or any control with a ScrollViewer) is scrolled down to its end :
/// the next results are fetched while browsing the list, as with the fetch button.
/// </summary>
public sealed class ScrollEndCommandBehavior : Behavior<Control>
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(ScrollEndCommandBehavior));

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        nameof(IsEnabled), typeof(bool), typeof(ScrollEndCommandBehavior), new PropertyMetadata(true));

    private readonly ScrollChangedEventHandler _scrollChanged;

    public ScrollEndCommandBehavior()
    {
        _scrollChanged = OnScrollChanged;
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>The command is executed only when enabled (same condition as the fetch button).</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(ScrollViewer.ScrollChangedEvent, _scrollChanged);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.RemoveHandler(ScrollViewer.ScrollChangedEvent, _scrollChanged);
        base.OnDetaching();
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // scrolled down by the user up to the end (a change of the content size alone is ignored)
        if (!IsEnabled || e.VerticalChange <= 0 || e.ExtentHeight - e.ViewportHeight - e.VerticalOffset > 1)
        {
            return;
        }
        if (Command is { } command && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }
}
