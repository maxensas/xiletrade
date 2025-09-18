using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Application.Serialization.Converter;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class Query
{
    [DataMember(Name = "status", Order = 0)]
    [JsonPropertyName("status")]
    [JsonPropertyOrder(0)]
    public OptionTxt Status { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [DataMember(Name = "type", EmitDefaultValue = false)]
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(QueryTypeJsonConverter))]
    public object Type { get; set; } // can be 'GemTransfigured' or string

    [DataMember(Name = "stats")]
    [JsonPropertyName("stats")]
    public Stats[] Stats { get; set; }

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public Filters Filters { get; set; } = new Filters();
}
