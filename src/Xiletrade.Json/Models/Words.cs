using System.Runtime.Serialization;

namespace XiletradeJson.Models
{
    [DataContract]
    internal sealed class Words
    {
        [DataMember(Name = "result")]
        public WordResult[]? Result { get; set; } = null;
    }

    [DataContract]
    internal sealed class WordResult
    {
        [DataMember(Name = "data")]
        public WordResultData[]? Data { get; set; } = null;
    }

    [DataContract]
    internal sealed class WordResultData
    {
        [DataMember(Name = "Text2")]
        internal string? Name { get; set; } = null;

        [DataMember(Name = "Text")]
        internal string? NameEn { get; set; } = null;
    }
}
