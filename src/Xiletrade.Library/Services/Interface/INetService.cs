using System.Net.Http;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services.Interface;

public interface INetService
{
    TradeCooldownHandler TradeCooldown { get; }

    void InitTradeClient(int timeout);

    Task<string> SendHTTP(string urlString, Client idClient);

    Task<string> SendHTTP(string entity, string urlString, Client idClient, bool isXml = false);

    HttpClient GetClient(Client idClient);

    Task<T> GetFromJsonAsync<T>(string urlString, Client idClient) where T : class, new();
}
