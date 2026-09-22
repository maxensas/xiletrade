namespace Xiletrade.Library.Services.Interface;

public interface IProtocolHandlerService
{
    /// <summary>
    /// Sends a protocol URL to the already running instance of the application.
    /// </summary>
    /// <param name="url">The custom protocol URL to send.</param>
    void SendToRunningInstance(string url); // Client
}
