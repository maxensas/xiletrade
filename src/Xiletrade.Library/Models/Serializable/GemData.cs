﻿using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemData
{
    [DataMember(Name = "result")]
    public GemResult[] Result { get; set; } = null;
}
