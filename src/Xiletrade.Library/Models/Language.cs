using Xiletrade.Library.Models.Enums;

namespace Xiletrade.Library.Models;

public sealed class Language(Lang id, string lang)
{
    public Lang Id { get; } = id;
    public string Lang { get; } = lang;
}
