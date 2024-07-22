using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ItemInfo
{
    [DataMember(Name = "currency")]
    public string Currency { get; set; } = string.Empty;

    [DataMember(Name = "amount")]
    public double Amount { get; set; } = 0;

    [DataMember(Name = "stock")]
    public int Stock { get; set; } = 0;

    [DataMember(Name = "whisper")]
    public string Whisper { get; set; } = string.Empty;
}
