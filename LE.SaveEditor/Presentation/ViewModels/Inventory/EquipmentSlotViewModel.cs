using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>A slot of the equipment grid; clicking it opens the item inspector for that slot.</summary>
public sealed partial class EquipmentSlotViewModel(EquipmentSlot slot, IIconProvider iconProvider) : ViewModelBase
{
    public EquipmentSlot Slot { get; } = slot;

    public string Label => Slot.DisplayName;

    /// <summary>Item read from the save or made in the builder; <c>null</c> while the slot is empty.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItem))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(HasIcon))]
    [NotifyPropertyChangedFor(nameof(RarityForeground))]
    [NotifyPropertyChangedFor(nameof(RarityBackground))]
    [NotifyPropertyChangedFor(nameof(ItemTooltip))]
    private BuiltItem? _item;

    public bool HasItem => Item is not null;

    public ImageSource? Icon => iconProvider.Get(Item?.Base.IconPath);

    public bool HasIcon => Icon is not null;

    public Brush? RarityForeground => Item is null ? null : RarityPalette.Foreground(Item.Rarity);

    public Brush? RarityBackground => Item is null ? null : RarityPalette.Background(Item.Rarity);

    /// <summary>Plain tooltip of an empty slot.</summary>
    public string ToolTip => $"{Label} — click to build an item";

    /// <summary>Game-styled tooltip of the item in the slot; <c>null</c> while the slot is empty.</summary>
    public ItemTooltipViewModel? ItemTooltip =>
        Item is null ? null : _itemTooltip ??= new ItemTooltipViewModel(Item, Icon, RarityPalette.Foreground(Item.Rarity));

    private ItemTooltipViewModel? _itemTooltip;

    partial void OnItemChanged(BuiltItem? value) => _itemTooltip = null;
}
