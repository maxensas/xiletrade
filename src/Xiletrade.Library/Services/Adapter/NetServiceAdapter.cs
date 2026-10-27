using Microsoft.Extensions.Logging;
using System;
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
        DataManagerService dm) : base(logger, token, dm)
    {
    }

    // TODO add other mock APIs.
    public override async Task<byte[]> SendHTTPBytes(ReadOnlyMemory<byte> json, string urlString, Client idClient, bool isXml = false)
    {
        if (idClient is Client.Trade)
        {
            await Task.Yield();
            return "{\"id\":\"bG2Xa5QRIL\",\"complexity\":22,\"result\":[],\"total\":0}"u8.ToArray();
        }

        return await base.SendHTTPBytes(json, urlString, idClient, isXml);
    }
}
