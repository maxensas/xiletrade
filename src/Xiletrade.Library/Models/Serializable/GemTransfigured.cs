using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemTransfigured
{
    [DataMember(Name = "option")]
    public string Option { get; set; } = null;

    [DataMember(Name = "discriminator")]
    public string Discriminator { get; set; } = null;
}
