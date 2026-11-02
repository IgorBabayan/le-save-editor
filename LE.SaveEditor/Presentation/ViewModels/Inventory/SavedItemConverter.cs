using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// Turns an item read from a save into the <see cref="BuiltItem"/> an equipment slot shows, and a built item back into
/// the <see cref="ItemData"/> the save stores.
/// </summary>
public static class SavedItemConverter
{
    /// <summary>Lowest tier that makes an affix exalted (T6–T8), as in the item builder.</summary>
    private const int ExaltedTier = 6;

    /// <summary>
    /// The item for <paramref name="slot"/>, or <c>null</c> when its base / unique is not in <c>base.db</c>
    /// (missing or outdated database). Affixes <c>base.db</c> does not know are left out.
    /// </summary>
    public static BuiltItem? ToBuiltItem(ItemData data, EquipmentSlot slot, IItemBaseCatalog catalog)
    {
        var itemBase = data.UniqueId is { } uniqueId
            ? catalog.FindUnique(uniqueId)
            : catalog.FindBase(data.BaseTypeId, data.SubTypeId);
        if (itemBase is null || itemBase.BaseTypeId != data.BaseTypeId)
            return null;

        var implicitValues = itemBase.Implicits.Select((stat, i) => FromRoll(stat, data.ImplicitRolls, i)).ToList();
        var modValues = itemBase.Mods.Select((mod, i) => FromRoll(mod, data.UniqueRolls, i)).ToList();

        var affixes = new List<BuiltAffix>(data.Affixes.Count);
        var counts = new Dictionary<AffixSlotKind, int>();
        foreach (var stored in data.Affixes)
        {
            if (catalog.FindAffix(stored.AffixId, data.BaseTypeId) is not { Tiers.Count: > 0 } affix)
                continue;

            var tier = Math.Clamp(stored.Tier, 1, affix.Tiers.Count);
            // The game keeps one roll per affix; every stat of the tier follows it.
            var values = affix.Tiers[tier - 1].Stats.Select(stat => ItemStatViewModel.FromRoll(stat, stored.Roll)).ToList();
            var kind = SlotKindOf(affix, stored);

            // Older editor versions wrote a normal affix used as the corruption as one more prefix / suffix (the game
            // shows it as such). A prefix / suffix that no longer fits its two slots goes into the corruption slot, so
            // applying the item again stores it as a real corruption.
            if (kind is AffixSlotKind.Prefix or AffixSlotKind.Suffix
                && counts.GetValueOrDefault(kind) >= MaxAffixesPerType
                && counts.GetValueOrDefault(AffixSlotKind.Corrupted) == 0)
                kind = AffixSlotKind.Corrupted;

            counts[kind] = counts.GetValueOrDefault(kind) + 1;
            affixes.Add(new BuiltAffix(kind, affix, tier, values));
        }

        return new BuiltItem(slot, RarityOf(data, affixes, slot.IsIdol), itemBase, implicitValues, modValues, affixes, data.ForgingPotential);
    }

    /// <summary>Prefixes (and suffixes) an item can have besides its sealed affix and corruption.</summary>
    private const int MaxAffixesPerType = 2;

    private static double FromRoll(ItemImplicit stat, IReadOnlyList<byte> rolls, int index) =>
        ItemStatViewModel.FromRoll(stat, index < rolls.Count ? rolls[index] : ItemStatViewModel.MaxRoll);

    private static AffixSlotKind SlotKindOf(AffixDefinition affix, ItemAffixData stored) => affix switch
    {
        _ when stored.IsCorrupted => AffixSlotKind.Corrupted,
        { OnlyOnUniques.Count: > 0 } => AffixSlotKind.Unique,
        { Category: AffixCategory.Corrupted } => AffixSlotKind.Corrupted,
        { Type: AffixType.Prefix } => stored.IsSealed ? AffixSlotKind.SealedPrefix : AffixSlotKind.Prefix,
        _ => stored.IsSealed ? AffixSlotKind.SealedSuffix : AffixSlotKind.Suffix
    };

    /// <summary>
    /// Unique, set and legendary keep the stored rarity; plain bases take theirs from the affixes like the item builder:
    /// Normal without affixes, Magic with 1–2, Rare with 3+, Exalted as soon as one affix is tier 6 or higher.
    /// Like in the game, the sealed affix and the corruption do not count.
    /// Idols are Normal without affixes and Magic with any.
    /// </summary>
    private static ItemRarity RarityOf(ItemData data, IReadOnlyList<BuiltAffix> affixes, bool isIdol)
    {
        switch (data.RarityCode)
        {
            case ItemDataReader.UniqueRarity:
                return ItemRarity.Unique;
            case ItemDataReader.SetRarity:
                return ItemRarity.Set;
            case ItemDataReader.LegendaryRarity:
                return ItemRarity.Legendary;
        }

        var counted = affixes.Where(a => a.Slot is AffixSlotKind.Prefix or AffixSlotKind.Suffix).ToList();
        if (isIdol)
            return counted.Count == 0 ? ItemRarity.Normal : ItemRarity.Magic;

        if (counted.Any(a => a.Tier >= ExaltedTier))
            return ItemRarity.Exalted;

        return counted.Count switch
        {
            0 => ItemRarity.Normal,
            <= 2 => ItemRarity.Magic,
            _ => ItemRarity.Rare
        };
    }

    /// <summary>
    /// The bytes to store for <paramref name="item"/>. <paramref name="original"/> is the item it replaces (if any):
    /// its format, instance id and stored bytes the editor does not edit are kept, and each roll whose value did not
    /// change keeps its stored byte, so an item that is applied again unchanged is written back exactly as it was.
    /// </summary>
    public static ItemData ToItemData(BuiltItem item, ItemData? original)
    {
        var itemBase = item.Base;
        var isUniqueKind = item.Rarity is ItemRarity.Unique or ItemRarity.Set or ItemRarity.Legendary;
        if (isUniqueKind && itemBase.UniqueId is null)
            throw new ArgumentException($"{itemBase.Name} is not a unique or set item.", nameof(item));

        // Hints are only valid for the very same base / unique (a unique is identified by its id; its stored sub type is
        // kept rather than trusting base.db for it).
        var sameBase = original is not null
                       && original.BaseTypeId == itemBase.BaseTypeId
                       && original.UniqueId == itemBase.UniqueId
                       && (isUniqueKind || original.SubTypeId == itemBase.SubTypeId);
        var sameKind = original is not null && original.IsUnique == isUniqueKind
                       && (original.RarityCode == ItemDataReader.LegendaryRarity) == (item.Rarity == ItemRarity.Legendary);

        var implicitRolls = new byte[ItemDataReader.ImplicitCount];
        for (var i = 0; i < implicitRolls.Length; i++)
        {
            byte? hint = sameBase && i < original!.ImplicitRolls.Count ? original.ImplicitRolls[i] : null;
            implicitRolls[i] = i < itemBase.Implicits.Count
                ? RollFor(itemBase.Implicits[i], item.ImplicitValues[i], hint)
                : hint ?? byte.MaxValue;
        }

        var uniqueRolls = Array.Empty<byte>();
        if (isUniqueKind)
        {
            uniqueRolls = new byte[ItemDataReader.UniqueRollCount];
            for (var i = 0; i < uniqueRolls.Length; i++)
            {
                byte? hint = sameBase && i < original!.UniqueRolls.Count ? original.UniqueRolls[i] : null;
                uniqueRolls[i] = i < itemBase.Mods.Count
                    ? RollFor(itemBase.Mods[i], item.ModValues[i], hint)
                    : hint ?? (byte)0;
            }
        }

        // The unique's own affixes before the others (the game shows them on top); the writer puts the sealed affix and
        // the corruption first and marks them, as the game does.
        IReadOnlyList<ItemAffixData> affixes = item.Rarity is ItemRarity.Unique or ItemRarity.Set
            ? []
            : item.Affixes
                .OrderBy(a => a.Slot == AffixSlotKind.Unique ? 0 : 1)
                .Select(a => ToAffixData(a, original))
                .ToList();
        var hasCorruption = affixes.Any(a => a.IsCorrupted);

        var rarityCode = item.Rarity switch
        {
            ItemRarity.Unique => ItemDataReader.UniqueRarity,
            ItemRarity.Set => ItemDataReader.SetRarity,
            ItemRarity.Legendary => ItemDataReader.LegendaryRarity,
            // Plain bases store the number of affixes that make their rarity (sealed and corruption do not count).
            _ => Math.Min(item.Affixes.Count(a => a.Slot is AffixSlotKind.Prefix or AffixSlotKind.Suffix), MaxPlainRarityCode)
        };

        var formatVersion = original?.FormatVersion ?? ItemDataReader.Version2;
        return new ItemData(
            itemBase.BaseTypeId,
            sameBase ? original!.SubTypeId : itemBase.SubTypeId,
            rarityCode,
            implicitRolls,
            isUniqueKind ? itemBase.UniqueId : null,
            uniqueRolls,
            isUniqueKind ? (int?)null : item.ForgingPotential ?? original?.ForgingPotential ?? 0,
            affixes,
            formatVersion,
            original?.InstanceId,
            FlagsOf(item, original, sameKind, hasCorruption),
            TailOf(item, original, sameKind, sameBase, formatVersion));
    }

    /// <summary>
    /// The stored flags of the same kind of item are kept; a new legendary gets the Temporal Sanctum flag unless its only
    /// affix is the corruption (the game writes a corrupted unique without it). The corrupted bit follows the corruption
    /// affix; an item the game corrupted without adding an affix stays corrupted.
    /// </summary>
    private static int FlagsOf(BuiltItem item, ItemData? original, bool sameKind, bool hasCorruption)
    {
        var flags = sameKind
            ? original!.Flags
            : item.Rarity == ItemRarity.Legendary && item.Affixes.Any(a => a.Slot != AffixSlotKind.Corrupted) ? LegendaryFlags : 0;

        if (hasCorruption)
            flags |= ItemDataReader.CorruptedItemFlag;
        else if (original?.Affixes.Any(a => a.IsCorrupted) == true)
            flags &= ~ItemDataReader.CorruptedItemFlag;
        return flags;
    }

    /// <summary>
    /// Uniques and set items end with their legendary potential (kept for the same unique). Items with affixes keep how
    /// the stored data ended; a new one ends after its affixes in format 6 and with a 0 in format 2.
    /// </summary>
    private static byte? TailOf(BuiltItem item, ItemData? original, bool sameKind, bool sameBase, int formatVersion)
    {
        if (item.Rarity is ItemRarity.Unique or ItemRarity.Set)
            return sameKind && sameBase ? original!.Tail ?? (byte)0 : (byte)0;

        if (sameKind)
            return original!.Tail;
        return formatVersion == ItemDataReader.Version2 ? (byte)0 : null;
    }

    /// <summary>Highest rarity code of a plain base (rare with four affixes).</summary>
    private const int MaxPlainRarityCode = 4;

    /// <summary>The flags byte the game writes on legendary items.</summary>
    private const int LegendaryFlags = 0x80;

    private static ItemAffixData ToAffixData(BuiltAffix affix, ItemData? original)
    {
        var stats = affix.TierData.Stats;
        var isSealed = affix.Slot is AffixSlotKind.SealedPrefix or AffixSlotKind.SealedSuffix;
        var isCorrupted = affix.Slot == AffixSlotKind.Corrupted;

        // The game keeps one roll per affix that every stat follows: keep the stored one while it still gives the values.
        var stored = original?.Affixes.FirstOrDefault(a => a.AffixId == affix.Affix.Id && a.Tier == affix.Tier);
        if (stored is not null
            && stats.Select((stat, i) => Same(ItemStatViewModel.FromRoll(stat, stored.Roll), affix.Values[i])).All(same => same))
            return new ItemAffixData(affix.Affix.Id, affix.Tier, stored.Roll, isSealed, isCorrupted);

        var roll = stats.Count > 0 ? affix.Rolls[0] : byte.MaxValue;
        return new ItemAffixData(affix.Affix.Id, affix.Tier, roll, isSealed, isCorrupted);
    }

    /// <summary>The stored <paramref name="hint"/> when it still gives <paramref name="value"/>, otherwise the roll of the value.</summary>
    private static byte RollFor(ItemImplicit stat, double value, byte? hint) =>
        hint is { } stored && Same(ItemStatViewModel.FromRoll(stat, stored), value)
            ? stored
            : ItemStatViewModel.ToRoll(stat, value);

    private static bool Same(double a, double b) => Math.Abs(a - b) <= 1e-9 * Math.Max(1, Math.Abs(b));
}
