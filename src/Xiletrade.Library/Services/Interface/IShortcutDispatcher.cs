using System;

namespace Xiletrade.Library.Services.Interface;

public interface IShortcutDispatcher
{
    Action<int, nint> HandleMessage {  get; }
}
