using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class SocketFilters
{
    [DataMember(Name = "sockets", EmitDefaultValue = false)]
    [JsonPropertyName("sockets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Sockets Sockets { get; set; } = new(); //public MinMax Sockets { get; set; } = new();

    [DataMember(Name = "links", EmitDefaultValue = false)]
    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Links { get; set; } = new();
}
