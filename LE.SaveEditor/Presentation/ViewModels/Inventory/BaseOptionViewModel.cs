using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// One base in the item builder list, grouped by the classes that can equip it:
/// the character's own class first, then bases for any class, then bases of other classes.
/// </summary>
public sealed class BaseOptionViewModel
{
    public BaseOptionViewModel(ItemBase itemBase, CharacterClass characterClass)
    {
        Base = itemBase;
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
            // Own class on top; other classes in the game's class order.
            GroupOrder = IsUsable ? 0 : 2 + (int)classes[0];
        }
    }

    public ItemBase Base { get; }

    public string Name => Base.Name;

    /// <summary>Header of the list group, e.g. "Mage" or "Any class".</summary>
    public string GroupName { get; }

    /// <summary>Position of the group: 0 own class, 1 any class, 2+ other classes.</summary>
    public int GroupOrder { get; }

    /// <summary>The character's class can equip this base; others are shown dimmed.</summary>
    public bool IsUsable { get; }

    public string LevelText => $"Lv {Base.LevelRequirement}";

    public string ToolTip => IsUsable
        ? Base.TypeName
        : $"{Base.TypeName} — only {GroupName} can equip this base";
}
