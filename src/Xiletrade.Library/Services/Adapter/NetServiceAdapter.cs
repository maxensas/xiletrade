using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Adapter;

/// <summary>
/// Adapter for <see cref="NetService"/> that allows mocking HTTP responses for testing purposes.
/// When <c>useMock</c> is true, it returns predefined mock responses instead of making real HTTP calls.
/// </summary>
public sealed class NetServiceAdapter : NetService
{
    /// <inheritdoc cref="NetServiceAdapter"/>
    /// <param name="service">The service provider used by the base <see cref="NetService"/> class.</param>
    public NetServiceAdapter(ILogger<NetService> logger, ITokenService token, 
        DataManagerService dm, PoeApiService poeApi) : base(logger, token, dm, poeApi)
    {
    }

    // TODO add other mock APIs.
    public override Task<string> SendHTTP(string urlString, Client idClient)
    {
        return base.SendHTTP(null, urlString, idClient);
    }

    public override async Task<string> SendHTTP(string entity, string urlString, Client idClient, bool isXml = false)
    {
        if (idClient is Client.Trade)
        {
            await Task.Yield();
            return $"{{\"id\":\"bG2Xa5QRIL\",\"complexity\":22,\"result\":[],\"total\":0}}";
        }

        return await base.SendHTTP(entity, urlString, idClient, isXml);
    }
}
