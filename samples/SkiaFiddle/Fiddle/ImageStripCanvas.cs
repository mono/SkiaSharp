using System;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace SkiaFiddle.Fiddle;

/// <summary>
/// A horizontal strip of image thumbnails (the embedded fiddle source images).
/// Tapping a thumbnail selects it; the selected image is what the snippet's
/// <c>image</c> variable points at. Drawn with Skia so it needs no XAML image
/// interop, and hit-tested by horizontal fraction so it is DPI independent.
/// Pointer input is captured by a transparent host in the page (a bare
/// SKCanvasElement does not reliably hit-test), which forwards taps to
/// <see cref="SelectFromPointer"/>.
/// </summary>
public class ImageStripCanvas : SKCanvasElement
{
    private int _selectedIndex = FiddleAssets.SelectedImageIndex;

    public event EventHandler<int>? SelectionChanged;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var clamped = Clamp(value);
            if (clamped == _selectedIndex)
                return;
            _selectedIndex = clamped;
            Invalidate();
        }
    }

    /// <summary>Selects the thumbnail under a horizontal position and raises <see cref="SelectionChanged"/>.</summary>
    public void SelectFromPointer(double x, double width)
    {
        var count = FiddleAssets.Images.Count;
        if (count == 0 || width <= 0)
            return;

        var index = Clamp((int)(x / width * count));
        if (index == _selectedIndex)
            return;

        _selectedIndex = index;
        Invalidate();
        SelectionChanged?.Invoke(this, index);
    }

    private static int Clamp(int value)
    {
        var count = FiddleAssets.Images.Count;
        if (count <= 0) return 0;
        if (value < 0) return 0;
        if (value >= count) return count - 1;
        return value;
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        canvas.Clear(new SKColor(0x11, 0x15, 0x1A));

        var images = FiddleAssets.Images;
        var count = images.Count;
        if (count == 0)
            return;

        var width = (float)area.Width;
        var height = (float)area.Height;
        var cellWidth = width / count;
        const float pad = 4f;

        using var border = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = new SKColor(0x23, 0x30, 0x3D),
            IsAntialias = true,
        };
        using var selected = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            Color = new SKColor(0x4F, 0xA3, 0xFF),
            IsAntialias = true,
        };
        using var checkerA = new SKPaint { Color = new SKColor(0x2A, 0x2A, 0x2A) };
        using var checkerB = new SKPaint { Color = new SKColor(0x20, 0x20, 0x20) };
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

        for (var i = 0; i < count; i++)
        {
            var image = images[i].Image;
            var cell = new SKRect(i * cellWidth + pad, pad, (i + 1) * cellWidth - pad, height - pad);

            canvas.Save();
            canvas.ClipRect(cell);
            DrawChecker(canvas, cell, checkerA, checkerB);
            canvas.DrawImage(image, Fit(image.Width, image.Height, cell), sampling);
            canvas.Restore();

            canvas.DrawRect(cell, i == _selectedIndex ? selected : border);
        }
    }

    private static void DrawChecker(SKCanvas canvas, SKRect rect, SKPaint a, SKPaint b)
    {
        const float size = 8f;
        var rows = (int)Math.Ceiling(rect.Height / size);
        var cols = (int)Math.Ceiling(rect.Width / size);
        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
            canvas.DrawRect(rect.Left + x * size, rect.Top + y * size, size, size, ((x + y) & 1) == 0 ? a : b);
    }

    private static SKRect Fit(int imageWidth, int imageHeight, SKRect cell)
    {
        if (imageWidth <= 0 || imageHeight <= 0)
            return cell;

        var scale = Math.Min(cell.Width / imageWidth, cell.Height / imageHeight);
        var w = imageWidth * scale;
        var h = imageHeight * scale;
        var cx = cell.MidX;
        var cy = cell.MidY;
        return new SKRect(cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2);
    }
}
