using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// One choice of the rarity picker in the item builder. <see cref="IsSelected"/> is set only by
/// <see cref="ItemBuilderViewModel"/>, so a declined change never moves the picker.
/// </summary>
public sealed partial class RarityOptionViewModel(ItemRarity rarity) : ViewModelBase
{
    public ItemRarity Rarity { get; } = rarity;

    public string Name => Rarity.ToString();

    public Brush Foreground => RarityPalette.Foreground(Rarity);

    public Brush Background => RarityPalette.Background(Rarity);

    [ObservableProperty]
    private bool _isSelected;
}
