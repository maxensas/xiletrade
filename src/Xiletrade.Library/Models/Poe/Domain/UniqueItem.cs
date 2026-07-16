namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class UniqueItem(string name, string text)
{
    public string Name { get; } = name;
    public string Text { get; } = text;
}
