using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main;

public sealed class MouseGestureCom(IRelayCommand command, ModifierKey modifier, MouseWheelDirection direction)
{
    public IRelayCommand Command { get; set; } = command;
    public ModifierKey Modifier { get; set; } = modifier;
    public MouseWheelDirection Direction { get; set; } = direction;
}
