using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Material.Icons;

namespace LE.SaveEditor.Presentation.Controls;

/// <summary>
/// A Material Design icon drawn from the path data of the <c>Material.Icons</c> package (the same icon set the WPF
/// version used). Drawn by the editor itself, so it does not tie the app to the Avalonia version an icon library was
/// built for. Each icon's geometry is parsed once and shared by every icon showing it.
/// </summary>
public sealed class MaterialIcon : Control
{
    /// <summary>Size of the Material Design canvas the path data is drawn on.</summary>
    private const double CanvasSize = 24;

    private static readonly ConcurrentDictionary<MaterialIconKind, Geometry?> Geometries = new();

    public static readonly StyledProperty<MaterialIconKind> KindProperty =
        AvaloniaProperty.Register<MaterialIcon, MaterialIconKind>(nameof(Kind));

    /// <summary>Color of the icon; inherited like the text color, so icons in buttons follow the button.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<MaterialIcon>();

    static MaterialIcon()
    {
        AffectsRender<MaterialIcon>(KindProperty, ForegroundProperty);
    }

    public MaterialIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>24 × 24 unless sized by Width / Height or a smaller space.</summary>
    protected override Size MeasureOverride(Size availableSize) =>
        new(Math.Min(CanvasSize, availableSize.Width), Math.Min(CanvasSize, availableSize.Height));

    public override void Render(DrawingContext context)
    {
        if (Foreground is not { } brush || GetGeometry(Kind) is not { } geometry)
            return;

        var size = Bounds.Size;
        var scale = Math.Min(size.Width, size.Height) / CanvasSize;
        if (scale <= 0)
            return;

        var offset = new Vector((size.Width - CanvasSize * scale) / 2, (size.Height - CanvasSize * scale) / 2);
        using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset)))
            context.DrawGeometry(brush, null, geometry);
    }

    private static Geometry? GetGeometry(MaterialIconKind kind) =>
        Geometries.GetOrAdd(kind, static k =>
        {
            var data = MaterialIconDataProvider.GetData(k);
            return string.IsNullOrEmpty(data) ? null : Geometry.Parse(data);
        });
}
