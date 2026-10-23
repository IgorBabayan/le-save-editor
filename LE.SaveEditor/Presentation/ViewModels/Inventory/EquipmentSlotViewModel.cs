using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>A slot of the equipment grid; clicking it opens the item builder for that slot.</summary>
public sealed partial class EquipmentSlotViewModel(EquipmentSlot slot, IIconProvider iconProvider) : ViewModelBase
{
    public EquipmentSlot Slot { get; } = slot;

    public string Label => Slot.DisplayName;

    /// <summary>Item made in the builder; <c>null</c> while the slot is empty (items are not parsed from the save yet).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItem))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(HasIcon))]
    [NotifyPropertyChangedFor(nameof(RarityForeground))]
    [NotifyPropertyChangedFor(nameof(RarityBackground))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private BuiltItem? _item;

    public bool HasItem => Item is not null;

    public ImageSource? Icon => iconProvider.Get(Item?.Base.IconPath);

    public bool HasIcon => Icon is not null;

    public Brush? RarityForeground => Item is null ? null : RarityPalette.Foreground(Item.Rarity);

    public Brush? RarityBackground => Item is null ? null : RarityPalette.Background(Item.Rarity);

    public string ToolTip
    {
        get
        {
            if (Item is null)
                return $"{Label} — click to build an item";

            var lines = new List<string> { Item.Base.Name, $"{Item.Rarity} {Item.Base.TypeName}" };
            if (Item.Base.BaseName is { } baseName)
                lines.Add(baseName);
            lines.AddRange(Item.Base.Implicits.Select((s, i) => ItemStatViewModel.Format(s, Item.ImplicitValues[i])));
            lines.AddRange(Item.Base.Mods.Select((s, i) => ItemStatViewModel.Format(s, Item.ModValues[i])));
            foreach (var affix in Item.Affixes)
            {
                var tag = affix.Slot switch
                {
                    AffixSlotKind.SealedPrefix or AffixSlotKind.SealedSuffix => " (sealed)",
                    AffixSlotKind.Corrupted => " (corrupted)",
                    _ => string.Empty
                };
                lines.AddRange(affix.Lines.Select(line => $"{line}  [T{affix.Tier}]{tag}"));
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
