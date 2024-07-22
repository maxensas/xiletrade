using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ParserData
{
    [DataMember(Name = "mods")]
    public ModOption[] Mods { get; set; } = null;
}
