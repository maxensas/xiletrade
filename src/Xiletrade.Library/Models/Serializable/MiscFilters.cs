﻿using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class MiscFilters
{
    [DataMember(Name = "quality")]
    public MinMax Quality { get; set; } = new MinMax();

    [DataMember(Name = "ilvl")]
    public MinMax Ilvl { get; set; } = new MinMax();

    [DataMember(Name = "gem_level")]
    public MinMax Gem_level { get; set; } = new MinMax();

    [DataMember(Name = "stored_experience")]
    public MinMax StoredExp { get; set; } = new MinMax();

    [DataMember(Name = "gem_alternate_quality", EmitDefaultValue = false)]
    public Options Gem_alternate { get; set; }

    [DataMember(Name = "corrupted", EmitDefaultValue = false)]
    public Options Corrupted { get; set; }
    /*
    [DataMember(Name = "shaper_item")]
    public Options Shaper { get; set; } = new Options();

    [DataMember(Name = "elder_item")]
    public Options Elder { get; set; } = new Options();

    [DataMember(Name = "crusader_item")]
    public Options Crusader { get; set; } = new Options();

    [DataMember(Name = "redeemer_item")]
    public Options Redeemer { get; set; } = new Options();

    [DataMember(Name = "hunter_item")]
    public Options Hunter { get; set; } = new Options();

    [DataMember(Name = "warlord_item")]
    public Options Warlord { get; set; } = new Options();
    */
    [DataMember(Name = "synthesised_item", EmitDefaultValue = false)]
    public Options Synthesis { get; set; }

    [DataMember(Name = "split", EmitDefaultValue = false)]
    public Options Split { get; set; }

    [DataMember(Name = "mirrored", EmitDefaultValue = false)]
    public Options Mirrored { get; set; }
}
