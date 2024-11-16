using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Options
{
    [DataMember(Name = "option", EmitDefaultValue = false)]
    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Option { get; set; }

    public Options()
    {
    }
    public Options(string opt)
    {
        Option = opt;
    }
}
