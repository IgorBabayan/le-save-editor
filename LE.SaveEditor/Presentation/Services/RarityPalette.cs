using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Services;

/// <summary>
/// Colors of item rarities as the game shows them. The same in every theme, since they identify the rarity;
/// backgrounds are translucent so they sit on both light and dark palettes.
/// Brushes are immutable and shared by every view (no per-item allocation, safe from any thread).
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

    private static readonly Dictionary<ItemRarity, IBrush> Foregrounds =
        Colors.ToDictionary(c => c.Key, IBrush (c) => new ImmutableSolidColorBrush(c.Value));

    private static readonly Dictionary<ItemRarity, IBrush> Backgrounds = Colors.ToDictionary(c => c.Key, IBrush (c) => new ImmutableLinearGradientBrush(
        [
            new ImmutableGradientStop(0, Color.FromArgb(0x70, c.Value.R, c.Value.G, c.Value.B)),
            new ImmutableGradientStop(1, Color.FromArgb(0x1C, c.Value.R, c.Value.G, c.Value.B))
        ],
        startPoint: new RelativePoint(0.5, 0, RelativeUnit.Relative),
        endPoint: new RelativePoint(0.5, 1, RelativeUnit.Relative)));

    /// <summary>Text and border color of the rarity.</summary>
    public static IBrush Foreground(ItemRarity rarity) => Foregrounds[rarity];

    /// <summary>Item background tinted with the rarity color.</summary>
    public static IBrush Background(ItemRarity rarity) => Backgrounds[rarity];
}
