﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable.SourceGeneration;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class AccountData
{
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "lastCharacterName")] // ggg does not respect snake case for this parameter.
    [JsonPropertyName("lastCharacterName")]
    public string LastCharacterName { get; set; } = string.Empty;

    [DataMember(Name = "language")]
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [DataMember(Name = "online")]
    [JsonPropertyName("online")]
    [JsonConverter(typeof(StatusConverter))]
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
