namespace Xiletrade.Library.Services.Interface;

public interface IProtocolHandlerService
{
    /// <summary>
    /// Starts a named pipe server that listens for protocol URLs sent by secondary instances.
    /// </summary>
    void StartListening(); // Server

    /// <summary>
    /// Stop listening server Task / Break infinite loop.
    /// </summary>
    void StopListening();

    /// <summary>
    /// Sends a protocol URL to the already running instance of the application.
    /// </summary>
    /// <param name="url">The custom protocol URL to send.</param>
    void SendToRunningInstance(string url); // Client
}
