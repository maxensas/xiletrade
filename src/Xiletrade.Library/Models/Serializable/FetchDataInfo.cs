using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FetchDataInfo
{
    [DataMember(Name = "id")]
    public string ID { get; set; } = string.Empty;

    [DataMember(Name = "listing")]
    public FetchDataListing Listing { get; set; } = new FetchDataListing();
}
