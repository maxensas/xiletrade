using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Windows.Input;

namespace Xiletrade.UI.Avalonia.Util.Command;

// AOT Compatible
public abstract class EventCommandBehaviorBase : Behavior<Control>
{
    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<EventCommandBehaviorBase, ICommand>(nameof(Command));

    public static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<EventCommandBehaviorBase, object>(nameof(CommandParameter));

    public static readonly StyledProperty<bool> PassAssociatedObjectProperty =
        AvaloniaProperty.Register<EventCommandBehaviorBase, bool>(nameof(PassAssociatedObject));

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Si true et qu'aucun CommandParameter n'est défini, la commande reçoit le contrôle associé (la fenêtre).</summary>
    public bool PassAssociatedObject
    {
        get => GetValue(PassAssociatedObjectProperty);
        set => SetValue(PassAssociatedObjectProperty, value);
    }

    protected abstract void Subscribe(Control target);
    protected abstract void Unsubscribe(Control target);

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is { } target)
            Subscribe(target);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is { } target)
            Unsubscribe(target);
        base.OnDetaching();
    }

    // Compatible with EventHandler and EventHandler<RoutedEventArgs> (contravariance)
    protected void OnEvent(object sender, EventArgs e)
    {
        var parameter = CommandParameter ?? (PassAssociatedObject ? AssociatedObject : null);
        if (Command?.CanExecute(parameter) == true)
            Command.Execute(parameter);
    }
}