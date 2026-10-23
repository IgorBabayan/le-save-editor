using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>Turns an item read from a save into the <see cref="BuiltItem"/> an equipment slot shows.</summary>
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
        foreach (var stored in data.Affixes)
        {
            if (catalog.FindAffix(stored.AffixId) is not { Tiers.Count: > 0 } affix)
                continue;

            var tier = Math.Clamp(stored.Tier, 1, affix.Tiers.Count);
            // The game keeps one roll per affix; every stat of the tier follows it.
            var values = affix.Tiers[tier - 1].Stats.Select(stat => ItemStatViewModel.FromRoll(stat, stored.Roll)).ToList();
            affixes.Add(new BuiltAffix(SlotKindOf(affix, stored.IsSealed), affix, tier, values));
        }

        return new BuiltItem(slot, RarityOf(data, affixes), itemBase, implicitValues, modValues, affixes, data.ForgingPotential);
    }

    private static double FromRoll(ItemImplicit stat, IReadOnlyList<byte> rolls, int index) =>
        ItemStatViewModel.FromRoll(stat, index < rolls.Count ? rolls[index] : ItemStatViewModel.MaxRoll);

    private static AffixSlotKind SlotKindOf(AffixDefinition affix, bool isSealed) => affix switch
    {
        { Category: AffixCategory.Corrupted } => AffixSlotKind.Corrupted,
        { Type: AffixType.Prefix } => isSealed ? AffixSlotKind.SealedPrefix : AffixSlotKind.Prefix,
        _ => isSealed ? AffixSlotKind.SealedSuffix : AffixSlotKind.Suffix
    };

    /// <summary>
    /// Unique, set and legendary keep the stored rarity; plain bases take theirs from the affixes like the item builder:
    /// Normal without affixes, Magic with 1–2, Rare with 3+, Exalted as soon as one affix is tier 6 or higher.
    /// </summary>
    private static ItemRarity RarityOf(ItemData data, IReadOnlyList<BuiltAffix> affixes)
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

        var counted = affixes.Where(a => a.Slot != AffixSlotKind.Corrupted).ToList();
        if (counted.Any(a => a.Tier >= ExaltedTier))
            return ItemRarity.Exalted;

        return counted.Count switch
        {
            0 => ItemRarity.Normal,
            <= 2 => ItemRarity.Magic,
            _ => ItemRarity.Rare
        };
    }
}
