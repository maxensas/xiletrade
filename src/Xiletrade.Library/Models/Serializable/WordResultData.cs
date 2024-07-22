using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResultData
{
    [DataMember(Name = "Text2")]
    public string Name { get; set; } = null;

    [DataMember(Name = "Text")]
    public string NameEn { get; set; } = null;
}
