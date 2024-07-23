using System.Runtime.Serialization;

namespace XiletradeJson.Models
{
    [DataContract]
    internal sealed class Gems
    {
        [DataMember(Name = "result")]
        public GemResult[]? Result { get; set; } = null;
    }

    [DataContract]
    internal sealed class GemResult
    {
        [DataMember(Name = "data")]
        public GemResultData[]? Data { get; set; } = null;
    }

    [DataContract]
    internal sealed class GemResultData
    {
        [DataMember(Name = "Id")]
        internal string? Id { get; set; } = null;

        [DataMember(Name = "Name")]
        internal string? Name { get; set; } = null;

        [DataMember(Name = "Type")]
        internal string? Type { get; set; } = null;

        [DataMember(Name = "Disc")]
        internal string? Disc { get; set; } = null;
    }
}
