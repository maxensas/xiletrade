using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ParserData
{
    [DataMember(Name = "mods")]
    [JsonPropertyName("mods")]
    public ModOption[] Mods { get; set; } = null;
}
