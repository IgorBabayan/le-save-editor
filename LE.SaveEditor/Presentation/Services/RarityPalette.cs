using System.Windows;
using System.Windows.Media;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Services;

/// <summary>
/// Colors of item rarities as the game shows them. The same in every theme, since they identify the rarity;
/// backgrounds are translucent so they sit on both light and dark palettes.
/// </summary>
public static class RarityPalette
{
    private static readonly IReadOnlyDictionary<ItemRarity, Color> Colors = new Dictionary<ItemRarity, Color>
    {
        [ItemRarity.Normal] = Color.FromRgb(0xC8, 0xC8, 0xC8),
        [ItemRarity.Magic] = Color.FromRgb(0x4F, 0x8F, 0xFF),
        [ItemRarity.Rare] = Color.FromRgb(0xF2, 0xD3, 0x45),
        [ItemRarity.Exalted] = Color.FromRgb(0xB2, 0x5C, 0xF2),
        [ItemRarity.Unique] = Color.FromRgb(0xF0, 0x86, 0x3A),
        [ItemRarity.Set] = Color.FromRgb(0x4C, 0xC9, 0x63),
        [ItemRarity.Legendary] = Color.FromRgb(0xE5, 0x4B, 0x3C)
    };

    private static readonly Dictionary<ItemRarity, Brush> Foregrounds = Colors.ToDictionary(c => c.Key, c => Freeze(new SolidColorBrush(c.Value)));

    private static readonly Dictionary<ItemRarity, Brush> Backgrounds = Colors.ToDictionary(c => c.Key, c => Freeze(new LinearGradientBrush(
        new GradientStopCollection
        {
            new(Color.FromArgb(0x70, c.Value.R, c.Value.G, c.Value.B), 0),
            new(Color.FromArgb(0x1C, c.Value.R, c.Value.G, c.Value.B), 1)
        },
        new Point(0.5, 0),
        new Point(0.5, 1))));

    /// <summary>Text and border color of the rarity.</summary>
    public static Brush Foreground(ItemRarity rarity) => Foregrounds[rarity];

    /// <summary>Item background tinted with the rarity color.</summary>
    public static Brush Background(ItemRarity rarity) => Backgrounds[rarity];

    private static Brush Freeze(Brush brush)
    {
        brush.Freeze();
        return brush;
    }
}
