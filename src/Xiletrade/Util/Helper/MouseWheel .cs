using System;
using System.Windows.Input;
using System.Windows.Markup;
using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Util.Helper;

/// <summary>Helper class used by XAML view.</summary>
public class MouseWheel : MarkupExtension
{
    public MouseWheelDirection Direction { get; set; }
    public ModifierKey Key { get; set; }

    public MouseWheel()
    {
        Key = ModifierKey.None;
        Direction = MouseWheelDirection.Down;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new MouseWheelGesture(Key, Direction);
    }
}


