using System.Windows.Media;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// A stat block of the item tooltip, laid out like the game's: the rolled lines, then the tier (affixes only),
/// then one "Range: min to max" line per rolled stat.
/// </summary>
/// <param name="Lines">Rolled lines, e.g. "22% increased Mana".</param>
/// <param name="TierText">"Tier: 7 (drop only)", or <c>null</c> for implicits and unique modifiers.</param>
/// <param name="Ranges">"Range: 21% to 26%", one per line.</param>
/// <param name="IsExalted">Tier 6 or higher: the lines are shown in the exalted color.</param>
public sealed record ItemTooltipEntry(
    IReadOnlyList<string> Lines,
    string? TierText,
    IReadOnlyList<string> Ranges,
    bool IsExalted)
{
    public bool HasTier => TierText is not null;
}

/// <summary>A titled group of the tooltip ("PREFIXES", "SUFFIXES", …).</summary>
public sealed record ItemTooltipSection(string Header, IReadOnlyList<ItemTooltipEntry> Entries);

/// <summary>
/// Game-styled tooltip of an equipped item: icon, name and type, implicits with their ranges, forging potential,
/// then unique modifiers, prefixes, suffixes, the sealed affix and the corruption, each affix with its tier and ranges.
/// </summary>
public sealed class ItemTooltipViewModel
{
    /// <summary>Lowest tier that only drops (cannot be crafted) and makes an item exalted.</summary>
    private const int ExaltedTier = 6;

    public ItemTooltipViewModel(BuiltItem item, ImageSource? icon, Brush rarityBrush)
    {
        Icon = icon;
        RarityBrush = rarityBrush;
        Title = NameOf(item).ToUpperInvariant();
        TypeLine = TypeLineOf(item.Base).ToUpperInvariant();

        Implicits = item.Base.Implicits
            .Select((stat, i) => StatEntry(stat, item.ImplicitValues[i]))
            .ToList();

        // Idols cannot be forged: their stored byte is not shown.
        if (item.ForgingPotential is { } forgingPotential && !item.Slot.IsIdol)
            ForgingPotentialText = $"{forgingPotential} Forging Potential";

        var sections = new List<ItemTooltipSection>();

        var mods = item.Base.Mods.Select((stat, i) => StatEntry(stat, item.ModValues[i])).ToList();
        if (mods.Count > 0)
            sections.Add(new ItemTooltipSection("MODIFIERS", mods));

        AddAffixSection(sections, "UNIQUE AFFIX", item, AffixSlotKind.Unique);
        AddAffixSection(sections, "PREFIXES", item, AffixSlotKind.Prefix);
        AddAffixSection(sections, "SUFFIXES", item, AffixSlotKind.Suffix);
        AddAffixSection(sections, "SEALED AFFIX", item, AffixSlotKind.SealedPrefix, AffixSlotKind.SealedSuffix);
        AddAffixSection(sections, "CORRUPTED", item, AffixSlotKind.Corrupted);
        Sections = sections;

        Effects = item.Base.Effects;

        var requirements = new List<string>();
        if (item.Base.LevelRequirement > 0)
            requirements.Add($"Requires Level {item.Base.LevelRequirement}");
        if (ClassNames(item.Base) is { } classes)
            requirements.Add($"Requires Class: {classes}");
        Requirements = requirements;
    }

    public ImageSource? Icon { get; }

    public bool HasIcon => Icon is not null;

    /// <summary>Name and border color of the item's rarity.</summary>
    public Brush RarityBrush { get; }

    /// <summary>"CERULEAN INFANTRY HAUBERK OF SPARKS".</summary>
    public string Title { get; }

    /// <summary>"SENTINEL BODY ARMOR"; for a unique / set item the base it is built on.</summary>
    public string TypeLine { get; }

    public IReadOnlyList<ItemTooltipEntry> Implicits { get; }

    /// <summary>"34 Forging Potential", or <c>null</c> when unknown (uniques, items made in the builder).</summary>
    public string? ForgingPotentialText { get; }

    public bool HasForgingPotential => ForgingPotentialText is not null;

    /// <summary>Unique modifiers, prefixes, suffixes, sealed affix and corruption; empty groups are left out.</summary>
    public IReadOnlyList<ItemTooltipSection> Sections { get; }

    /// <summary>Lines of a unique / set item that do not roll.</summary>
    public IReadOnlyList<string> Effects { get; }

    public bool HasEffects => Effects.Count > 0;

    /// <summary>"Requires Level 30", "Requires Class: Sentinel".</summary>
    public IReadOnlyList<string> Requirements { get; }

    public bool HasRequirements => Requirements.Count > 0;

    private static ItemTooltipEntry StatEntry(ItemImplicit stat, double value) =>
        new([ItemStatViewModel.Format(stat, value)], null, [Range(stat)], IsExalted: false);

    private static string Range(ItemImplicit stat) => $"Range: {ItemStatViewModel.FormatBounds(stat)}";

    private static void AddAffixSection(List<ItemTooltipSection> sections, string header, BuiltItem item, params AffixSlotKind[] kinds)
    {
        var entries = item.Affixes
            .Where(a => kinds.Contains(a.Slot))
            .Select(a => new ItemTooltipEntry(
                a.Lines.ToList(),
                a.Tier >= ExaltedTier ? $"Tier: {a.Tier} (drop only)" : $"Tier: {a.Tier}",
                a.TierData.Stats.Select(Range).ToList(),
                a.Tier >= ExaltedTier))
            .ToList();

        if (entries.Count > 0)
            sections.Add(new ItemTooltipSection(header, entries));
    }

    /// <summary>
    /// Uniques, sets and normal items keep their name; magic, rare and exalted items are named like in the game:
    /// first prefix title + base name + first suffix title ("Cerulean Infantry Hauberk of Sparks").
    /// </summary>
    private static string NameOf(BuiltItem item)
    {
        if (item.Base.IsUnique || item.Rarity is ItemRarity.Normal or ItemRarity.Unique or ItemRarity.Set or ItemRarity.Legendary)
            return item.Base.Name;

        var prefix = item.Affixes
            .Where(a => a.Slot is AffixSlotKind.Prefix or AffixSlotKind.SealedPrefix)
            .Select(a => a.Affix.Title)
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
        var suffix = item.Affixes
            .Where(a => a.Slot is AffixSlotKind.Suffix or AffixSlotKind.SealedSuffix)
            .Select(a => a.Affix.Title)
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

        return string.Join(" ", new[] { prefix, item.Base.Name, suffix }.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string TypeLineOf(ItemBase itemBase) =>
        itemBase.BaseName
        ?? (ClassNames(itemBase) is { } classes ? $"{classes} {itemBase.TypeName}" : itemBase.TypeName);

    /// <summary>"Sentinel", "Mage / Acolyte", or <c>null</c> when any class can equip the item.</summary>
    private static string? ClassNames(ItemBase itemBase)
    {
        if (itemBase.ClassRequirement == 0)
            return null;

        var classes = Enum.GetValues<CharacterClass>()
            .Where(c => (itemBase.ClassRequirement & (1 << (int)c)) != 0)
            .Select(CharacterClassCatalog.GetDisplayName)
            .ToList();
        return classes.Count == 0 ? null : string.Join(" / ", classes);
    }
}
