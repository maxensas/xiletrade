using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class CurrencyResult
{
    [DataMember(Name = "result")]
    public CurrencyResultData[] Result { get; set; } = null;
}
