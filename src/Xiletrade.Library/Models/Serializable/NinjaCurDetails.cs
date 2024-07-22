using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaCurDetails
{
    [DataMember(Name = "id")]
    public int Id { get; set; } = 0;

    [DataMember(Name = "icon")]
    public string Icon { get; set; } = null;

    [DataMember(Name = "name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "tradeId")]
    public string TradeId { get; set; } = null;
}
