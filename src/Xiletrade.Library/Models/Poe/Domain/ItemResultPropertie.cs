using System.Globalization;

namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class ItemResultPropertie(string name, string value)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
    public string MaxQualityValue { get; set; }

    public ItemResultPropertie(string name, decimal value) : this(name, value.ToString(CultureInfo.InvariantCulture))
    {

    }

    public ItemResultPropertie(string name, string value, string maxQualValue) : this(name, value)
    {
        MaxQualityValue = maxQualValue;
    }
}
