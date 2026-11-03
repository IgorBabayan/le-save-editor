using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The gold, Circle of Fortune favor, ancient bones and the material storage of one stash file with the changes made in the editor. Every character that uses the stash
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

    /// <summary>The stash as read from (or last written to) its file.</summary>
    public StashSave Stash { get; private set; } = stash;

    /// <summary>The amounts, the gold, the favor or the ancient bones changed, or the stash was saved.</summary>
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

    /// <summary>The gold, the favor, the ancient bones or some amount differs from the file.</summary>
    public bool HasChanges =>
        Currencies != Stash.Currencies
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

    /// <summary>The stash was written: <paramref name="saved"/> (read back from the file) is the new state.</summary>
    public void MarkSaved(StashSave saved)
    {
        Stash = saved;
        _amounts = new Dictionary<CraftingMaterialKey, int>(saved.Amounts);
        _gold = saved.Gold;
        _favor = saved.Favor ?? 0;
        _ancientBones = saved.AncientBones;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
