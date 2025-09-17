namespace Xiletrade.Library.Models;

public sealed class ItemResultPropertie(string name, decimal? value)
{
    public string Name { get; set; } = name;
    public decimal? Value { get; set; } = value;
}
