using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class TradeFilters
{
    [DataMember(Name = "indexed", EmitDefaultValue = false)]
    public Options Indexed { get; set; }

    [DataMember(Name = "sale_type", EmitDefaultValue = false)]
    public Options SaleType { get; set; }

    [DataMember(Name = "price", EmitDefaultValue = false)]
    public MinMax Price { get; set; } = new();
}
