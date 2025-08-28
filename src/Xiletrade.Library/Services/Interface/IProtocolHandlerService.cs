namespace Xiletrade.Library.Services.Interface;

public interface IProtocolHandlerService
{
    /// <summary>
    /// Handles a custom protocol URL received at startup or from another instance.
    /// </summary>
    /// <param name="url">The URL to handle (e.g. "Xiletrade://open/item123").</param>
    void HandleUrl(string url);

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
