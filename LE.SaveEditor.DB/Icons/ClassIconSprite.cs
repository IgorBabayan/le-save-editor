using System.Reflection;
using LE.SaveEditor.DB.Infrastructure;
using SkiaSharp;

namespace LE.SaveEditor.DB.Icons;

/// <summary>
/// Class and mastery icons. They are not part of the <c>icons</c> node of <c>data.json</c>, so the generator ships
/// one sprite sheet (<c>Assets/class-icons.webp</c>) and cuts it into <c>Icons/classes/{name}.png</c>,
/// where the editor finds them by name.
/// </summary>
/// <remarks>
/// The sheet is a 4 × 5 grid of 64 px cells separated by 1 px transparent gaps (260 × 325 px).
/// </remarks>
internal sealed class ClassIconSprite(ConfigPaths paths)
{
    /// <summary>Folder under <c>Icons</c> the cut icons are written to.</summary>
    public const string Group = "classes";

    private const string ResourceName = "LE.SaveEditor.DB.class-icons.webp";

    private const int CellSize = 64;
    private const int Gap = 1;
    private const int Columns = 4;
    private const int Rows = 5;

    /// <summary>File name (without extension) of every cell, by column and row.</summary>
    private static readonly (string Name, int Column, int Row)[] Cells =
    [
        ("necromancer", 0, 0), ("paladin", 1, 0), ("lich", 2, 0), ("sentinel", 3, 0),
        ("mage", 0, 1), ("forge_guard", 1, 1), ("void_knight", 2, 1), ("warlock", 3, 1),
        ("rogue", 0, 2), ("spellblade", 1, 2), ("runemaster", 2, 2), ("druid", 3, 2),
        ("primalist", 0, 3), ("falconer", 1, 3), ("bladedancer", 2, 3), ("beastmaster", 3, 3),
        ("sorcerer", 0, 4), ("shaman", 1, 4), ("marksman", 2, 4), ("acolyte", 3, 4)
    ];

    /// <summary>
    /// Writes one PNG per class / mastery. Must run after <see cref="IconSynchronizer"/>,
    /// which deletes every file under <c>Icons</c> that is not in the <c>Icons</c> table.
    /// </summary>
    /// <returns><c>null</c> on success, otherwise why the icons could not be made (the run itself still succeeds).</returns>
    public async Task<string?> ExtractAsync(CancellationToken ct)
    {
        using var bar = new ConsoleProgressBar("Class icons");

        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            bar.Complete("sprite missing");
            return $"embedded resource {ResourceName} was not found";
        }

        using var sheet = SKBitmap.Decode(stream);
        if (sheet is null)
        {
            bar.Complete("not an image");
            return $"{ResourceName} is not a readable image";
        }

        var expectedWidth = Columns * (CellSize + Gap);
        var expectedHeight = Rows * (CellSize + Gap);
        if (sheet.Width != expectedWidth || sheet.Height != expectedHeight)
        {
            bar.Complete("unexpected layout");
            return $"{ResourceName}: expected {expectedWidth}×{expectedHeight} px, got {sheet.Width}×{sheet.Height} px";
        }

        try
        {
            var folder = Path.Combine(paths.IconsDirectory, Group);
            Directory.CreateDirectory(folder);

            for (var i = 0; i < Cells.Length; i++)
            {
                ct.ThrowIfCancellationRequested();
                var (name, column, row) = Cells[i];

                using var cell = new SKBitmap(CellSize, CellSize, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (var canvas = new SKCanvas(cell))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(
                        sheet,
                        SKRect.Create(column * (CellSize + Gap), row * (CellSize + Gap), CellSize, CellSize),
                        SKRect.Create(0, 0, CellSize, CellSize));
                }

                using var image = SKImage.FromBitmap(cell);
                using var png = image.Encode(SKEncodedImageFormat.Png, 100)
                                ?? throw new InvalidDataException($"PNG encoding of {name} failed");
                await using (var target = File.Create(Path.Combine(folder, name + ".png")))
                    png.SaveTo(target);

                bar.Report(i + 1, Cells.Length, name);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            bar.Complete("failed");
            return $"class icons: {ex.Message}";
        }

        bar.Complete($"{Cells.Length} icons");
        return null;
    }
}
