using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResultData
{
    [DataMember(Name = "Name")] // Text2
    public string Name { get; set; } = null;

    [DataMember(Name = "NameEn")] // Text
    public string NameEn { get; set; } = null;
}
