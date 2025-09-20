using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class TradeFilters
{
    [DataMember(Name = "indexed", EmitDefaultValue = false)]
    [JsonPropertyName("indexed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Indexed { get; set; }

    [DataMember(Name = "sale_type", EmitDefaultValue = false)]
    [JsonPropertyName("sale_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt SaleType { get; set; }

    [DataMember(Name = "price", EmitDefaultValue = false)]
    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Price { get; set; } = new();
}
