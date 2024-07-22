using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class CurrencyEntrie
{
    [DataMember(Name = "id")]
    public string ID { get; set; } = null;

    [DataMember(Name = "text")]
    public string Text { get; set; } = null;

    [DataMember(Name = "image", EmitDefaultValue = false)]
    public string Img { get; set; } = null;
}
