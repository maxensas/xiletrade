using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Services.Interface;

public interface IMessageAdapterService
{
    public void Show(string message, string caption, MessageStatus status);
}
