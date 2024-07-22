using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class CurrencyResultData
{
    [DataMember(Name = "id")]
    public string ID { get; set; } = null;

    [DataMember(Name = "label")]
    public string Label { get; set; } = null;

    [DataMember(Name = "entries")]
    public CurrencyEntrie[] Entries { get; set; } = null;
}
