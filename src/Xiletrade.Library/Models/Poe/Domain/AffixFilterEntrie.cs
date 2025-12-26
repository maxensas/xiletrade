namespace Xiletrade.Library.Models.Poe.Domain;

public sealed class AffixFilterEntrie(string id, string name, string type, bool isCorruptionImplicit, bool isUniqueExplicit, bool isMutated = false)
{
    public string ID { get; } = id;
    public string Name { get; } = name;
    public string Type { get; } = type?.Length > 0 ? type : id.Split('.')[0];
    public bool IsCorruptionImplicit { get; } = isCorruptionImplicit;
    public bool IsUniqueExplicit { get; } = isUniqueExplicit;
    public bool IsMutatedExplicit { get; } = isMutated;
}
