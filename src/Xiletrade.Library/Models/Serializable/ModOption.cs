using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ModOption
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "stat")]
    public string Stat { get; set; } = string.Empty;

    [DataMember(Name = "replace")]
    public string Replace { get; set; } = string.Empty;

    [DataMember(Name = "old")]
    public string Old { get; set; } = string.Empty;

    [DataMember(Name = "new")]
    public string New { get; set; } = string.Empty;
}
