using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The gold, Circle of Fortune favor, ancient bones, faction ranks, woven echoes and the material storage of one stash file with the changes made in the editor. Every character that uses the stash
/// shares this instance (softcore characters of a cycle share their stash), so a change shows for all of them and is
/// written once.
/// </summary>
public sealed class StashMaterialsViewModel(StashSave stash)
{
    /// <summary>Most gold the editor writes (the game keeps it in a 32-bit number).</summary>
    public const int MaxGold = int.MaxValue;

    /// <summary>Most Circle of Fortune favor the editor writes (the game keeps it in a 32-bit number).</summary>
    public const int MaxFavor = int.MaxValue;

    /// <summary>Most ancient bones the editor writes (the game keeps them in a 32-bit number).</summary>
    public const int MaxAncientBones = int.MaxValue;

    private Dictionary<CraftingMaterialKey, int> _amounts = new(stash.Amounts);
    private int _gold = stash.Gold;
    private int _favor = stash.Favor ?? 0;
    private int _ancientBones = stash.AncientBones;
    private Dictionary<string, int> _factionRanks = new(stash.FactionRanks, StringComparer.Ordinal);
    private Dictionary<int, int> _wovenEchoes = new(stash.WovenEchoes);

    /// <summary>The stash as read from (or last written to) its file.</summary>
    public StashSave Stash { get; private set; } = stash;

    /// <summary>The amounts, the gold, the favor, the ancient bones, a faction rank or a woven echo changed, or the stash was saved.</summary>
    public event EventHandler? Changed;

    /// <summary>Every material of the storage with its current amount (with entries the game keeps at 0).</summary>
    public IReadOnlyDictionary<CraftingMaterialKey, int> Amounts => _amounts;

    /// <summary>Gold of the stash with the change made in the editor.</summary>
    public int Gold => _gold;

    /// <summary>The stash has a Circle of Fortune entry, so its favor can be changed.</summary>
    public bool HasFavor => Stash.Favor is not null;

    /// <summary>Circle of Fortune favor of the stash with the change made in the editor (0 without an entry).</summary>
    public int Favor => _favor;

    /// <summary>Ancient bones of the stash with the change made in the editor.</summary>
    public int AncientBones => _ancientBones;

    /// <summary>Gold, favor and ancient bones with the changes made in the editor.</summary>
    public StashCurrencies Currencies => new(_gold, _favor, _ancientBones);

    /// <summary>Rank of every faction the stash has an entry for, with the changes made in the editor (by faction key).</summary>
    public IReadOnlyDictionary<string, int> FactionRanks => _factionRanks;

    /// <summary>The faction <paramref name="factionId"/> is unlocked: the stash has an entry for it.</summary>
    public bool HasFaction(string factionId) => Stash.FactionRanks.ContainsKey(factionId);

    /// <summary>Rank of the faction <paramref name="factionId"/> with the change made in the editor; <c>null</c> when it is locked.</summary>
    public int? GetFactionRank(string factionId) =>
        _factionRanks.TryGetValue(factionId, out var rank) ? rank : null;

    /// <summary>
    /// Amount of every woven echo of the "Woven Echoes" tab, by echo (<c>subTypeID</c> of item type 107), with the changes
    /// made in the editor.
    /// </summary>
    public IReadOnlyDictionary<int, int> WovenEchoes => _wovenEchoes;

    /// <summary>The "Woven Echoes" tab has been unlocked in game by a character of the stash.</summary>
    public bool IsWovenEchoesTabUnlocked => Stash.IsWovenEchoesTabUnlocked;

    /// <summary>The gold, the favor, the ancient bones, a faction rank, a woven echo or some amount differs from the file.</summary>
    public bool HasChanges =>
        Currencies != Stash.Currencies
        || _wovenEchoes.Keys.Concat(Stash.WovenEchoes.Keys).Distinct()
            .Any(id => _wovenEchoes.GetValueOrDefault(id) != Stash.WovenEchoes.GetValueOrDefault(id))
        || _factionRanks.Any(r => Stash.FactionRanks.GetValueOrDefault(r.Key) != r.Value)
        || _amounts.Keys.Concat(Stash.Amounts.Keys).Distinct()
            .Any(key => _amounts.GetValueOrDefault(key) != Stash.Amounts.GetValueOrDefault(key));

    /// <summary>How many materials of <paramref name="kind"/> the storage holds now.</summary>
    public int Total(CraftingMaterialKind kind) => _amounts.Where(a => a.Key.Kind == kind).Sum(a => a.Value);

    /// <summary>Amount of each material of <paramref name="kind"/> the storage holds, by sub type.</summary>
    public IReadOnlyDictionary<int, int> GetAmounts(CraftingMaterialKind kind) =>
        _amounts.Where(a => a.Key.Kind == kind && a.Value > 0).ToDictionary(a => a.Key.SubTypeId, a => a.Value);

    /// <summary>Replaces every material of <paramref name="kind"/> with <paramref name="amounts"/> (by sub type).</summary>
    public void SetAmounts(CraftingMaterialKind kind, IReadOnlyDictionary<int, int> amounts)
    {
        var updated = _amounts.Where(a => a.Key.Kind != kind).ToDictionary(a => a.Key, a => a.Value);
        foreach (var (subTypeId, quantity) in amounts)
        {
            if (quantity > 0)
                updated[new CraftingMaterialKey(kind, subTypeId)] = quantity;
        }

        // Entries the game keeps at 0 stay "unchanged" as long as the user does not add the material.
        foreach (var (key, quantity) in Stash.Amounts)
        {
            if (quantity == 0 && key.Kind == kind && !updated.ContainsKey(key) && !amounts.ContainsKey(key.SubTypeId))
                updated[key] = 0;
        }

        _amounts = updated;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the amount of one material (0 removes it on the next save; an entry the game keeps at 0 stays unchanged).
    /// Nothing happens when the amount is the current one.
    /// </summary>
    public void SetAmount(CraftingMaterialKey key, int quantity)
    {
        quantity = Math.Max(0, quantity);
        if (_amounts.GetValueOrDefault(key) == quantity && (quantity > 0 || _amounts.ContainsKey(key) == Stash.Amounts.ContainsKey(key)))
            return;

        var updated = new Dictionary<CraftingMaterialKey, int>(_amounts);
        if (quantity > 0 || Stash.Amounts.ContainsKey(key))
            updated[key] = quantity;
        else
            updated.Remove(key);

        _amounts = updated;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the amount of the woven echo <paramref name="echoId"/> (0 removes it on the next save); nothing happens when it
    /// is the current one.
    /// </summary>
    public void SetWovenEchoAmount(int echoId, int quantity)
    {
        quantity = Math.Max(0, quantity);
        if (_wovenEchoes.GetValueOrDefault(echoId) == quantity)
            return;

        var updated = new Dictionary<int, int>(_wovenEchoes);
        if (quantity > 0)
            updated[echoId] = quantity;
        else
            updated.Remove(echoId);

        _wovenEchoes = updated;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sets the gold of the stash (limited to 0 … <see cref="MaxGold"/>); nothing happens when it is the current one.</summary>
    public void SetGold(int gold)
    {
        gold = Math.Clamp(gold, 0, MaxGold);
        if (gold == _gold)
            return;

        _gold = gold;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the Circle of Fortune favor of the stash (limited to 0 … <see cref="MaxFavor"/>); nothing happens when it is the
    /// current one or the stash has no Circle of Fortune entry.
    /// </summary>
    public void SetFavor(int favor)
    {
        favor = Math.Clamp(favor, 0, MaxFavor);
        if (!HasFavor || favor == _favor)
            return;

        _favor = favor;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sets the ancient bones of the stash (limited to 0 … <see cref="MaxAncientBones"/>); nothing happens when it is the current one.</summary>
    public void SetAncientBones(int ancientBones)
    {
        ancientBones = Math.Clamp(ancientBones, 0, MaxAncientBones);
        if (ancientBones == _ancientBones)
            return;

        _ancientBones = ancientBones;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the rank of the faction <paramref name="factionId"/> (limited to 0 … <paramref name="maxRank"/>); nothing
    /// happens when it is the current one or the stash has no entry for the faction (it is not unlocked).
    /// </summary>
    public void SetFactionRank(string factionId, int rank, int maxRank)
    {
        rank = Math.Clamp(rank, 0, Math.Max(0, maxRank));
        if (!_factionRanks.TryGetValue(factionId, out var current) || current == rank)
            return;

        _factionRanks = new Dictionary<string, int>(_factionRanks, StringComparer.Ordinal) { [factionId] = rank };
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The stash was written: <paramref name="saved"/> (read back from the file) is the new state.</summary>
    public void MarkSaved(StashSave saved)
    {
        Stash = saved;
        _amounts = new Dictionary<CraftingMaterialKey, int>(saved.Amounts);
        _gold = saved.Gold;
        _favor = saved.Favor ?? 0;
        _ancientBones = saved.AncientBones;
        _factionRanks = new Dictionary<string, int>(saved.FactionRanks, StringComparer.Ordinal);
        _wovenEchoes = new Dictionary<int, int>(saved.WovenEchoes);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
