using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Interface;

public interface INotificationService
{
    void Send(string title, string message, Notify type);
}