using Avalonia.Controls;

namespace Xiletrade.Updater.Util.Command;

public sealed class WindowClosingCommandBehavior : EventCommandBehaviorBase
{
    protected override void Subscribe(Control t) { if (t is Window w) w.Closing += OnEvent; }
    protected override void Unsubscribe(Control t) { if (t is Window w) w.Closing -= OnEvent; }
}