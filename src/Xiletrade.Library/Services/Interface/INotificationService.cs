using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Services.Interface;

public interface INotificationService
{
    void Send(string title, string message, Notify type);
}