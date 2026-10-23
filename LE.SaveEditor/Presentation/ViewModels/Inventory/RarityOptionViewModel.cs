using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>One choice of the rarity picker in the item builder.</summary>
public sealed class RarityOptionViewModel(ItemRarity rarity)
{
    public ItemRarity Rarity { get; } = rarity;

    public string Name => Rarity.ToString();

    public Brush Foreground => RarityPalette.Foreground(Rarity);

    public Brush Background => RarityPalette.Background(Rarity);
}
