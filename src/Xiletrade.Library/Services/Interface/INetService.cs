using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Interface;

/// <summary>Provides HTTP client management and request operations for Xiletrade.</summary>
public interface INetService
{
    void InitTradeClient(int timeout);

    /// <summary>
    /// Gets the last rate limit state received for a policy of the trade api.
    /// </summary>
    /// <returns>Null when no response has given it yet.</returns>
    RateLimitInfo GetRateLimit(string policy);

    /// <summary>Sends a GET request and returns the UTF-8 body as it is received (no string conversion).</summary>
    Task<byte[]> SendHTTPBytes(string urlString, Client idClient);

    /// <summary>Sends a request (POST when <paramref name="json"/> is not empty) and returns the UTF-8 body as it is received.</summary>
    /// <param name="json">Json body, in UTF-8.</param>
    /// <param name="referer">Page of the trade site the request comes from, some tokens are checked with it (hideout).</param>
    Task<byte[]> SendHTTPBytes(ReadOnlyMemory<byte> json, string urlString, Client idClient, bool isXml = false, string referer = null);

    /// <summary>
    /// Opens a websocket of the trade site (live search) : same session cookie and user agent as the trade requests.
    /// </summary>
    /// <exception cref="HttpRequestException">Connection refused, with the status code of the server.</exception>
    Task<WebSocket> ConnectTradeSocketAsync(string urlString, CancellationToken cancellation);

    HttpClient GetClient(Client idClient);

    Task<T> GetFromJsonAsync<T>(string urlString, Client idClient) where T : class, new();
}
