using System.Runtime.Serialization;

namespace XiletradeJson.Models
{
    [DataContract]
    internal class Bases
    {
        [DataMember(Name = "result")]
        internal Result[]? Result { get; set; }
    }

    [DataContract]
    internal sealed class Result
    {
        [DataMember(Name = "data")]
        internal ResultData[]? Data { get; set; }
    }

    [DataContract]
    internal sealed class ResultData
    {
        [DataMember(Name = "Id")]
        internal string? ID { get; set; }

        [DataMember(Name = "NameEn")]
        internal string? NameEn { get; set; }

        [DataMember(Name = "Name")]
        internal string? Name { get; set; }

        [DataMember(Name = "InheritsFrom")]
        internal string? InheritsFrom { get; set; }
    }
}
