namespace Xiletrade.Library.Models;

public sealed class ItemFilter
{
    public string Id { get; set; }
    public string Text { get; set; }
    public double Max { get; set; }
    public double Min { get; set; }
    public bool Disabled { get; set; }
    public int Option { get; set; }
    public bool IsNull { get; set; }

    public ItemFilter()
    {

    }
    public ItemFilter(string id)
    {
        Id = id;
    }
}
