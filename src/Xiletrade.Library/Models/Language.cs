namespace Xiletrade.Library.Models;

public sealed class Language(int id, string lang)
{
    public int Id { get; set; } = id;
    public string Lang { get; set; } = lang;
}
