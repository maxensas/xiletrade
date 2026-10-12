using Avalonia.Controls;

namespace Xiletrade.UI.Avalonia.Util.Command;

public sealed class WindowDeactivatedCommandBehavior : EventCommandBehaviorBase
{
    protected override void Subscribe(Control t) { if (t is Window w) w.Deactivated += OnEvent; }
    protected override void Unsubscribe(Control t) { if (t is Window w) w.Deactivated -= OnEvent; }
}