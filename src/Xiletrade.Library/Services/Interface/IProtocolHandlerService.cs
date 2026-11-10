namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Handles communication between application instances through a custom protocol.
/// </summary>
public interface IProtocolHandlerService
{
    const string ProtocolName = "Xiletrade";

    /// <summary>
    /// Registers the protocol, listens for the URLs of secondary instances and handles the URL of the startup.
    /// </summary>
    /// <remarks>Primary instance only.</remarks>
    void Start(); // Server

    /// <summary>
    /// Sends a protocol URL to the already running instance of the application.
    /// </summary>
    /// <param name="url">The custom protocol URL to send.</param>
    void SendToRunningInstance(string url); // Client
}
