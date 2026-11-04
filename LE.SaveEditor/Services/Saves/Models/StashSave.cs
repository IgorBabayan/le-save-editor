using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>The amounts the stash keeps outside its storage: gold, Circle of Fortune favor and ancient bones.</summary>
public readonly record struct StashCurrencies(int Gold, int Favor, int AncientBones);

/// <summary>A crafting material in the material storage: its kind and sub type (for a shard, the <c>affixId</c>).</summary>
public readonly record struct CraftingMaterialKey(CraftingMaterialKind Kind, int SubTypeId);

/// <summary>
/// The material storage of one stash file (<c>STASH_0</c>, <c>STASH_CYCLE_8_0</c>, <c>STASH_CYCLE_8_2_1</c>, ...): what the
/// game's "Crafting Materials" panel shows. Affix shards are the <c>savedShards</c> entries
/// (<c>{"shardType": affixId, "quantity": n}</c>), runes and glyphs the <c>materialsList</c> entries (an item with a
/// quantity, e.g. <c>"data": [2, 102, 3]</c> = Rune of Discovery). The resources of the game's "Resources" inventory tab
/// (keys, charms, primordial materials, ...) are the <c>keysList</c> entries: an item in format 6 in container 100 at the
/// resource's slot (<c>"data": [6, id, id, id, id, 104, 4], "inventoryPosition": {"x": 0, "y": 4}, "containerID": 100</c>
/// = Temporal Sanctum Key). The stash also keeps the gold of its characters (<c>"gold": 2191</c>), their ancient bones
/// (<c>"ancientBones": 0</c>), their factions with the Circle of Fortune favor (<c>"factions": {"0": {"rank": 1, "favor": 104, ...}}</c>) and the
/// blessings they have discovered (<c>"unlockedBlessings": [111, 8, ...]</c>, blessing <c>subTypeID</c>s). The woven echoes
/// of the game's "Woven Echoes" tab are the <c>wovenEchoesList</c> entries (<c>[6, id, id, id, id, 107, 0, echo]</c> in
/// container 99 at x 0, y = echo; see <see cref="WovenEchoData"/>), and <c>wovenEchoesTabUnlocked</c> tells whether
/// the tab has been unlocked in game.
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

    /// <summary><c>ancientBones</c>: the ancient bones of the stash, which every character using it shares.</summary>
    public int AncientBones { get; init; }

    /// <summary>
    /// <c>factions["0"].favor</c>: the Circle of Fortune favor of the stash; <c>null</c> when the stash has no Circle of
    /// Fortune entry (none of its characters has met the guild yet), so there is no favor to edit.
    /// </summary>
    public int? Favor { get; init; }

    /// <summary>
    /// <c>factions[id].rank</c> of every faction entry the stash has, by faction key (<c>"0"</c> = Circle of Fortune). A
    /// faction without an entry has not been unlocked in game by any character of the stash.
    /// </summary>
    public IReadOnlyDictionary<string, int> FactionRanks { get; init; } = new Dictionary<string, int>();

    /// <summary>Gold, ancient bones and favor of the stash.</summary>
    public StashCurrencies Currencies => new(Gold, Favor ?? 0, AncientBones);

    /// <summary>
    /// <c>unlockedBlessings</c>: blessings (<c>subTypeID</c> of item type 34) the characters of the stash have discovered.
    /// The game shows a blessing a character holds as "Undiscovered" unless its stash lists it here.
    /// </summary>
    public IReadOnlyList<int> UnlockedBlessings { get; init; } = [];

    /// <summary>
    /// <c>wovenEchoesList</c>: amount of every woven echo of the "Woven Echoes" tab, by echo (<c>subTypeID</c> of item type
    /// 107; entries of the same echo are added up).
    /// </summary>
    public IReadOnlyDictionary<int, int> WovenEchoes { get; init; } = new Dictionary<int, int>();

    /// <summary><c>wovenEchoesTabUnlocked</c>: a character of the stash has unlocked the "Woven Echoes" tab in game.</summary>
    public bool IsWovenEchoesTabUnlocked { get; init; }

    /// <summary>Amount of every material in the storage (entries of the same material are added up).</summary>
    public IReadOnlyDictionary<CraftingMaterialKey, int> Amounts { get; init; } = new Dictionary<CraftingMaterialKey, int>();

    /// <summary>Fingerprint of the content the stash was read from; saving refuses to overwrite a changed file.</summary>
    public string ContentHash { get; init; } = string.Empty;

    public string FileName => System.IO.Path.GetFileName(FilePath);

    /// <summary>How many materials of <paramref name="kind"/> the storage holds.</summary>
    public int Total(CraftingMaterialKind kind) => Amounts.Where(a => a.Key.Kind == kind).Sum(a => a.Value);
}
