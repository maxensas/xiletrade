using System;
using System.Threading.Tasks;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Adapter;

/// <summary>
/// Adapter for <see cref="NetService"/> that allows mocking HTTP responses for testing purposes.
/// When <c>useMock</c> is true, it returns predefined mock responses instead of making real HTTP calls.
/// </summary>
internal sealed class NetServiceAdapter : NetService
{
    private readonly bool _useMock;

    /// <inheritdoc cref="NetServiceAdapter"/>
    /// <param name="service">The service provider used by the base <see cref="NetService"/> class.</param>
    /// <param name="useMock">Indicates whether to use mock responses.</param>
    internal NetServiceAdapter(IServiceProvider service, bool useMock) : base(service)
    {
        _useMock = useMock;
    }

    // TODO add other mock APIs.
    internal override Task<string> SendHTTP(string urlString, Client idClient)
    {
        /*
        if (_useMock && idClient is Client.Trade)
        {
            return Task.FromResult($"{{\"id\":\"bG2Xa5QRIL\",\"complexity\":22,\"result\":[],\"total\":0}}");
        }
        */
        return base.SendHTTP(null, urlString, idClient);
    }

    internal override async Task<string> SendHTTP(string entity, string urlString, Client idClient)
    {
        if (_useMock && idClient is Client.Trade)
        {
            await Task.Yield();
            return $"{{\"id\":\"bG2Xa5QRIL\",\"complexity\":22,\"result\":[],\"total\":0}}";
        }

        return await base.SendHTTP(entity, urlString, idClient);
    }
}
