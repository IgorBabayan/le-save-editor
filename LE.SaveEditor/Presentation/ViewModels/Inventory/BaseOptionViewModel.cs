using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// One card in the item builder grid, laid out like the game's item tooltip (icon, name, type, stat ranges,
/// requirements). Cards are grouped by the classes that can equip them: the character's own class first,
/// then items for any class, then items of other classes.
/// </summary>
public sealed class BaseOptionViewModel
{
    private readonly IIconProvider _iconProvider;
    private IReadOnlyList<string>? _statLines;

    public BaseOptionViewModel(ItemBase itemBase, CharacterClass characterClass, IIconProvider iconProvider)
    {
        Base = itemBase;
        _iconProvider = iconProvider;
        IsUsable = itemBase.IsUsableBy(characterClass);

        var classes = Enum.GetValues<CharacterClass>()
            .Where(c => (itemBase.ClassRequirement & (1 << (int)c)) != 0)
            .ToList();

        if (itemBase.ClassRequirement == 0 || classes.Count == 0)
        {
            GroupName = "Any class";
            GroupOrder = 1;
        }
        else
        {
            GroupName = string.Join(" / ", classes.Select(CharacterClassCatalog.GetDisplayName));
            ClassText = $"Requires Class: {GroupName}";
            // Own class on top; other classes in the game's class order.
            GroupOrder = IsUsable ? 0 : 2 + (int)classes[0];
        }
    }

    public ItemBase Base { get; }

    public string Name => Base.Name;

    /// <summary>Card title, upper-case like the game's tooltip.</summary>
    public string Title => Base.Name.ToUpperInvariant();

    /// <summary>
    /// "PRIMALIST BODY ARMOR" for a class base, "BODY ARMOR" for any class; for a unique / set item the base it is
    /// built on ("JEWELLED CIRCLET").
    /// </summary>
    public string TypeLine => (Base.BaseName
                               ?? (ClassText is null ? Base.TypeName : $"{GroupName} {Base.TypeName}")).ToUpperInvariant();

    /// <summary>Implicits, then unique modifiers, each with its whole roll range ("+(1–4) Attunement").</summary>
    public IReadOnlyList<string> StatLines => _statLines ??= Base.Implicits
        .Concat(Base.Mods)
        .Select(ItemStatViewModel.FormatRange)
        .ToList();

    /// <summary>Effects of a unique / set item that do not roll.</summary>
    public IReadOnlyList<string> Effects => Base.Effects;

    public ImageSource? Icon => _iconProvider.Get(Base.IconPath);

    /// <summary>Header of the grid group, e.g. "Mage" or "Any class".</summary>
    public string GroupName { get; }

    /// <summary>Position of the group: 0 own class, 1 any class, 2+ other classes.</summary>
    public int GroupOrder { get; }

    /// <summary>The character's class can equip this item; others are shown dimmed.</summary>
    public bool IsUsable { get; }

    /// <summary>"Requires Level 6"; empty for level 0 (the game shows no requirement then).</summary>
    public string LevelText => Base.LevelRequirement > 0 ? $"Requires Level {Base.LevelRequirement}" : string.Empty;

    public bool HasLevelRequirement => Base.LevelRequirement > 0;

    /// <summary>"Requires Class: Primalist", or <c>null</c> when any class can equip the item.</summary>
    public string? ClassText { get; }

    public bool HasClassRequirement => ClassText is not null;

    public string? ToolTip => IsUsable ? null : $"Only {GroupName} can equip this item";
}
