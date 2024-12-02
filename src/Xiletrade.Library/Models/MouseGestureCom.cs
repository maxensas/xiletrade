using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Models;

public sealed class MouseGestureCom(IRelayCommand command, ModifierKey modifier, MouseWheelDirection direction)
{
    public IRelayCommand Command { get; set; } = command;
    public ModifierKey Modifier { get; set; } = modifier;
    public MouseWheelDirection Direction { get; set; } = direction;
}
