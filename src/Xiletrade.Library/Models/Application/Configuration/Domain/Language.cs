using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.Configuration.Domain;

public sealed class Language(Lang id, string lang)
{
    public Lang Id { get; } = id;
    public string Lang { get; } = lang;
}
