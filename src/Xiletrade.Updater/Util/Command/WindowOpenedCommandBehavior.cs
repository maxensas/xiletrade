using Avalonia.Controls;

namespace Xiletrade.Updater.Util.Command;

public sealed class WindowOpenedCommandBehavior : EventCommandBehaviorBase
{
    protected override void Subscribe(Control t) { if (t is Window w) w.Opened += OnEvent; }
    protected override void Unsubscribe(Control t) { if (t is Window w) w.Opened -= OnEvent; }
}
