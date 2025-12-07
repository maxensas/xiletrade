using System.Text.Json.Serialization;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class AccountData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lastCharacterName")] // ggg does not respect snake case for this parameter.
    public string LastCharacterName { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("online")]
    public OnlineStatus Online { get; set; }

    [JsonIgnore]
    public TradeStatus Status { 
        get 
        {
            if (Online is null)
            {
                return TradeStatus.Async;
            }

            if (Online.Status is not null)
            {
                if (Online.Status is Strings.Status.Online)
                {
                    return TradeStatus.Online;
                }
                if (Online.Status is Strings.Status.Afk)
                {
                    return TradeStatus.Afk;
                }
                if (Online.Status is Strings.Status.Offline)
                {
                    return TradeStatus.Offline;
                }
            }

            return TradeStatus.Error;
        } 
    }
}
