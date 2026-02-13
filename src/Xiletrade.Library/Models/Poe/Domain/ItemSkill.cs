namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class ItemSkill(string icon, string label)
{
    public string Icon { get; set; } = icon;
    public string Label { get; set; } = label;
}
