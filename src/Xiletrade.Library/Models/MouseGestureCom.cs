using System.Windows.Input;
using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Models;

public sealed class MouseGestureCom(ICommand command, ModifierKey modifier, MouseWheelDirection direction)
{
    public ICommand Command { get; set; } = command;
    public ModifierKey Modifier { get; set; } = modifier;
    public MouseWheelDirection Direction { get; set; } = direction;
}
