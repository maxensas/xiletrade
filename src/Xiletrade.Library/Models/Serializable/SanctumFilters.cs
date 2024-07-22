using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class SanctumFilters
{
    [DataMember(Name = "sanctum_resolve", EmitDefaultValue = false)]
    public MinMax Resolve { get; set; } = new();

    [DataMember(Name = "sanctum_max_resolve", EmitDefaultValue = false)]
    public MinMax MaxResolve { get; set; } = new();

    [DataMember(Name = "sanctum_inspiration", EmitDefaultValue = false)]
    public MinMax Inspiration { get; set; } = new();

    [DataMember(Name = "sanctum_gold", EmitDefaultValue = false)]
    public MinMax Aureus { get; set; } = new();
}
