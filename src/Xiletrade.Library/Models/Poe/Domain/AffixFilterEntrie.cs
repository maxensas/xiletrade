namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class AffixFilterEntrie(string id, string name, bool isCorruptionImplicit, bool isUniqueExplicit, bool isMutated = false)
{
    public string ID { get; } = id;
    public string Name { get; } = name;
    public bool IsCorruptionImplicit { get; } = isCorruptionImplicit;
    public bool IsUniqueExplicit { get; } = isUniqueExplicit;
    public bool IsMutatedExplicit { get; } = isMutated;
}
