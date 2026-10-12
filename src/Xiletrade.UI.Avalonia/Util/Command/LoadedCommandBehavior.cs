using Avalonia.Controls;

namespace Xiletrade.UI.Avalonia.Util.Command;

public sealed class LoadedCommandBehavior : EventCommandBehaviorBase
{
    protected override void Subscribe(Control t) => t.Loaded += OnEvent;
    protected override void Unsubscribe(Control t) => t.Loaded -= OnEvent;
}