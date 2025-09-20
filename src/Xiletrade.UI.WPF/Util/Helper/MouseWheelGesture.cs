using System.Windows.Input;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.WPF.Util.Helper;

public class MouseWheelGesture : MouseGesture
{
    public MouseWheelDirection Direction { get; set; }

    public MouseWheelGesture(ModifierKey keys, MouseWheelDirection direction)
        : base(MouseAction.WheelClick, GetModifierAdapter(keys))
    {
        Direction = direction;
    }

    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
    {
        var args = inputEventArgs as MouseWheelEventArgs;
        if (args is null)
            return false;
        if (!base.Matches(targetElement, inputEventArgs))
            return false;
        if (Direction == MouseWheelDirection.Up && args.Delta > 0
            || Direction == MouseWheelDirection.Down && args.Delta < 0)
        {
            inputEventArgs.Handled = true;
            return true;
        }

        return false;
    }

    private static ModifierKeys GetModifierAdapter(ModifierKey modifier)
    {
        return modifier is ModifierKey.Control ? ModifierKeys.Control 
            : modifier is ModifierKey.Alt ? ModifierKeys.Alt
            : modifier is ModifierKey.Shift ? ModifierKeys.Shift
            : ModifierKeys.None;
    }
}
