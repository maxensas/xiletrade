using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemResultData
{
    [DataMember(Name = "Id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "Name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "Type")]
    public string Type { get; set; } = null;

    [DataMember(Name = "Disc")]
    public string Disc { get; set; } = null;
}
