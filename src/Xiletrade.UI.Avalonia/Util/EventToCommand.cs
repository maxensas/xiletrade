using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace Xiletrade.UI.Avalonia.Util;

public static class EventToCommand
{
    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand>(
            "Command", typeof(EventToCommand));

    public static void SetCommand(AvaloniaObject element, ICommand value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand GetCommand(AvaloniaObject element) =>
        element.GetValue(CommandProperty);

    public static readonly AttachedProperty<string> EventNameProperty =
        AvaloniaProperty.RegisterAttached<Control, string>(
            "EventName", typeof(EventToCommand));

    public static void SetEventName(AvaloniaObject element, string value) =>
        element.SetValue(EventNameProperty, value);

    public static string GetEventName(AvaloniaObject element) =>
        element.GetValue(EventNameProperty);

    static EventToCommand()
    {
        CommandProperty.Changed.AddClassHandler<Control>((control, e) =>
        {
            var command = GetCommand(control);
            var eventName = GetEventName(control);

            if (control is ComboBox combo && eventName == "SelectionChanged")
            {
                combo.SelectionChanged += (_, _) => command?.Execute(combo.SelectedItem);
            }

            if (control is Button btn && eventName == "Click")
            {
                btn.Click += (_, _) => command?.Execute(null);
            }

            // Add more controls/events if needed
        });
    }
}