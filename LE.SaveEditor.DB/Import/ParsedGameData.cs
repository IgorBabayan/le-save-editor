using LE.SaveEditor.DB.Data;
using LE.SaveEditor.DB.Data.Entities;

namespace LE.SaveEditor.DB.Import;

/// <summary>Records of every stored node, ready to be written into <c>base.db</c>.</summary>
internal sealed record ParsedGameData(string SourceSha256, IReadOnlyDictionary<GameNode, IReadOnlyList<GameEntity>> Records)
{
    public int TotalRecords => Records.Values.Sum(r => r.Count);

    public IReadOnlyList<Icon> Icons => Records[GameNode.Icons].Cast<Icon>().ToList();
}
