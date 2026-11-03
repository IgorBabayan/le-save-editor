using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>A crafting material in the material storage: its kind and sub type (for a shard, the <c>affixId</c>).</summary>
public readonly record struct CraftingMaterialKey(CraftingMaterialKind Kind, int SubTypeId);

/// <summary>
/// The material storage of one stash file (<c>STASH_0</c>, <c>STASH_CYCLE_8_0</c>, <c>STASH_CYCLE_8_2_1</c>, ...): what the
/// game's "Crafting Materials" panel shows. Affix shards are the <c>savedShards</c> entries
/// (<c>{"shardType": affixId, "quantity": n}</c>), runes and glyphs the <c>materialsList</c> entries (an item with a
/// quantity, e.g. <c>"data": [2, 102, 3]</c> = Rune of Discovery). The resources of the game's "Resources" inventory tab
/// (keys, charms, primordial materials, ...) are the <c>keysList</c> entries: an item in format 6 in container 100 at the
/// resource's slot (<c>"data": [6, id, id, id, id, 104, 4], "inventoryPosition": {"x": 0, "y": 4}, "containerID": 100</c>
/// = Temporal Sanctum Key). The stash also keeps the gold of its characters (<c>"gold": 2191</c>).
/// </summary>
public sealed record StashSave
{
    /// <summary>Full path of the stash file.</summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// <c>stashType</c>: 0 = softcore, 1 = hardcore, 2 = the own stash of a solo challenge character
    /// (<see cref="SoloCharacterId"/>).
    /// </summary>
    public int StashType { get; init; }

    /// <summary><c>soloChallengeCharacterId</c>: the character id (<c>id</c> of its save) a solo stash belongs to.</summary>
    public string? SoloCharacterId { get; init; }

    /// <summary><c>gold</c>: the gold of the stash, which every character using it shares.</summary>
    public int Gold { get; init; }

    /// <summary>Amount of every material in the storage (entries of the same material are added up).</summary>
    public IReadOnlyDictionary<CraftingMaterialKey, int> Amounts { get; init; } = new Dictionary<CraftingMaterialKey, int>();

    /// <summary>Fingerprint of the content the stash was read from; saving refuses to overwrite a changed file.</summary>
    public string ContentHash { get; init; } = string.Empty;

    public string FileName => System.IO.Path.GetFileName(FilePath);

    /// <summary>How many materials of <paramref name="kind"/> the storage holds.</summary>
    public int Total(CraftingMaterialKind kind) => Amounts.Where(a => a.Key.Kind == kind).Sum(a => a.Value);
}
