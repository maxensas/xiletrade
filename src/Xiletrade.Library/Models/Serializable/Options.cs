using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Options
{
    [DataMember(Name = "option", EmitDefaultValue = false)]
    public string Option { get; set; }

    public Options()
    {
    }
    public Options(string opt)
    {
        Option = opt;
    }
}
