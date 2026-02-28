using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace SkiaSharpSample.Pages;

/// <summary>
/// Drawing Canvas Demo — touch/pointer drawing with wheel brush size control,
/// pressure sensitivity, color picker, and overlaid toolbar.
/// </summary>
public partial class Drawing
{
    SKCanvasView skiaView = null!;
    float brushSize = 25f;
    SKColor currentColor = SKColors.Black;
    List<DrawingStroke> strokes = new();
    DrawingStroke? currentStroke = null;
    SKPoint lastPoint;

    static readonly (string Name, SKColor Color)[] colors =
    [
        ("black", SKColors.Black),
        ("red", SKColors.Red),
        ("blue", SKColors.Blue),
        ("green", SKColors.Green),
        ("orange", SKColors.Orange),
        ("purple", SKColors.Purple),
    ];

    void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
        };

        // Draw completed strokes
        foreach (var stroke in strokes)
        {
            paint.Color = stroke.Color;
            paint.StrokeWidth = stroke.StrokeWidth;
            canvas.DrawPath(stroke.Path, paint);
        }

        // Draw in-progress stroke
        if (currentStroke != null)
        {
            paint.Color = currentStroke.Color;
            paint.StrokeWidth = currentStroke.StrokeWidth;
            canvas.DrawPath(currentStroke.Path, paint);
        }

        // Brush size indicator at last pointer position
        paint.Color = SKColors.Gray;
        paint.StrokeWidth = 1;
        canvas.DrawCircle(lastPoint, brushSize / 2f, paint);
    }

    void OnTouch(SKTouchEventArgs e)
    {
        lastPoint = e.Location;

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                currentStroke = new DrawingStroke(new SKPath(), currentColor) { StrokeWidth = brushSize };
                currentStroke.Path.MoveTo(e.Location);
                e.Handled = true;
                break;

            case SKTouchAction.Moved:
                if (currentStroke != null)
                {
                    currentStroke.Path.LineTo(e.Location);
                    e.Handled = true;
                }
                break;

            case SKTouchAction.Released:
                if (currentStroke != null)
                {
                    strokes.Add(currentStroke);
                    currentStroke = null;
                }
                e.Handled = true;
                break;

            case SKTouchAction.WheelChanged:
                brushSize += e.WheelDelta / 120f;
                brushSize = Math.Clamp(brushSize, 1f, 50f);
                e.Handled = true;
                break;
        }

        skiaView.Invalidate();
        StateHasChanged();
    }

    void SetColor(SKColor color)
    {
        currentColor = color;
        skiaView.Invalidate();
        StateHasChanged();
    }

    void ClearCanvas()
    {
        foreach (var stroke in strokes)
            stroke.Path.Dispose();
        strokes.Clear();
        currentStroke?.Path.Dispose();
        currentStroke = null;
        skiaView.Invalidate();
        StateHasChanged();
    }

    // Represents a single drawing stroke with its path, color, and width.
    // Path is owned — callers must dispose it when removing strokes.
    record DrawingStroke(SKPath Path, SKColor Color)
    {
        public float StrokeWidth { get; set; }
    }
}
