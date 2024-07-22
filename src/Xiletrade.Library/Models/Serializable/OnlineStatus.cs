using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class OnlineStatus
{
    [DataMember(Name = "league")]
    public string League { get; set; } = string.Empty;
    [DataMember(Name = "status")]
    public string Status { get; set; } = string.Empty; // null or afk
}
