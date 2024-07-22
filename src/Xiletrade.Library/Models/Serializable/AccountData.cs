using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class AccountData
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "lastCharacterName")]
    public string LastCharName { get; set; } = string.Empty;

    [DataMember(Name = "language")]
    public string Language { get; set; } = string.Empty;

    [DataMember(Name = "online")]
    public OnlineStatus Online { get; set; } = null;
}
