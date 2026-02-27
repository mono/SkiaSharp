using SkiaSharp;

namespace SkiaSharpSample;

/// <summary>
/// Shared SkiaSharp drawing logic used by all rendering approaches.
/// </summary>
public static class SkiaRenderer
{
    public static void Draw(SKCanvas canvas, SKImageInfo info, string variant)
    {
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            IsAntialias = true,
        };
        using var font = new SKFont
        {
            Size = 24,
        };

        var centerX = info.Width / 2f;
        var centerY = info.Height / 2f;

        switch (variant)
        {
            case "circles":
                DrawCircles(canvas, info, paint);
                break;
            case "clock":
                DrawClock(canvas, info, paint, font);
                break;
            default:
                DrawDefault(canvas, info, paint, font);
                break;
        }

        // Label the variant in the bottom-right corner
        paint.Color = SKColors.Gray;
        font.Size = 12;
        canvas.DrawText($"variant: {variant}", info.Width - 10, info.Height - 10, SKTextAlign.Right, font, paint);
    }

    private static void DrawDefault(SKCanvas canvas, SKImageInfo info, SKPaint paint, SKFont font)
    {
        paint.Color = SKColors.CornflowerBlue;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 80, paint);

        paint.Color = SKColors.White;
        font.Size = 28;
        canvas.DrawText("SkiaSharp SSR", info.Width / 2f, info.Height / 2f + 10, SKTextAlign.Center, font, paint);
    }

    private static void DrawCircles(SKCanvas canvas, SKImageInfo info, SKPaint paint)
    {
        var colors = new[] { SKColors.Red, SKColors.Orange, SKColors.Gold, SKColors.Green, SKColors.CornflowerBlue, SKColors.Purple };
        var centerX = info.Width / 2f;
        var centerY = info.Height / 2f;
        var maxRadius = Math.Min(info.Width, info.Height) * 0.4f;

        for (int i = 0; i < colors.Length; i++)
        {
            paint.Color = colors[i].WithAlpha(180);
            paint.Style = SKPaintStyle.Fill;

            var angle = (float)(i * 2 * Math.PI / colors.Length);
            var x = centerX + (float)Math.Cos(angle) * maxRadius * 0.3f;
            var y = centerY + (float)Math.Sin(angle) * maxRadius * 0.3f;

            canvas.DrawCircle(x, y, maxRadius * 0.5f, paint);
        }
    }

    private static void DrawClock(SKCanvas canvas, SKImageInfo info, SKPaint paint, SKFont font)
    {
        var centerX = info.Width / 2f;
        var centerY = info.Height / 2f;
        var clockRadius = Math.Min(info.Width, info.Height) * 0.4f;
        var now = DateTime.Now;

        // Clock face
        paint.Color = SKColors.LightGray;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(centerX, centerY, clockRadius, paint);

        paint.Color = SKColors.Black;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 3;
        canvas.DrawCircle(centerX, centerY, clockRadius, paint);

        // Hour markers
        for (int i = 0; i < 12; i++)
        {
            var angle = (float)(i * 2 * Math.PI / 12) - MathF.PI / 2;
            var innerR = clockRadius * 0.85f;
            var outerR = clockRadius * 0.95f;
            canvas.DrawLine(
                centerX + MathF.Cos(angle) * innerR,
                centerY + MathF.Sin(angle) * innerR,
                centerX + MathF.Cos(angle) * outerR,
                centerY + MathF.Sin(angle) * outerR,
                paint);
        }

        // Hour hand
        paint.StrokeWidth = 4;
        paint.StrokeCap = SKStrokeCap.Round;
        var hourAngle = (float)((now.Hour % 12 + now.Minute / 60.0) * 2 * Math.PI / 12) - MathF.PI / 2;
        canvas.DrawLine(centerX, centerY,
            centerX + MathF.Cos(hourAngle) * clockRadius * 0.5f,
            centerY + MathF.Sin(hourAngle) * clockRadius * 0.5f,
            paint);

        // Minute hand
        paint.StrokeWidth = 2;
        var minuteAngle = (float)((now.Minute + now.Second / 60.0) * 2 * Math.PI / 60) - MathF.PI / 2;
        canvas.DrawLine(centerX, centerY,
            centerX + MathF.Cos(minuteAngle) * clockRadius * 0.7f,
            centerY + MathF.Sin(minuteAngle) * clockRadius * 0.7f,
            paint);

        // Second hand
        paint.Color = SKColors.Red;
        paint.StrokeWidth = 1;
        var secondAngle = (float)(now.Second * 2 * Math.PI / 60) - MathF.PI / 2;
        canvas.DrawLine(centerX, centerY,
            centerX + MathF.Cos(secondAngle) * clockRadius * 0.8f,
            centerY + MathF.Sin(secondAngle) * clockRadius * 0.8f,
            paint);

        // Center dot
        paint.Color = SKColors.Black;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(centerX, centerY, 5, paint);

        // Time text
        paint.Color = SKColors.DarkGray;
        font.Size = 16;
        canvas.DrawText(now.ToString("HH:mm:ss"), centerX, centerY + clockRadius + 30, SKTextAlign.Center, font, paint);
    }

    /// <summary>
    /// Renders to a WebP byte array (for inline base64 embedding).
    /// </summary>
    public static byte[] RenderToWebP(int width, int height, string variant, int quality = 90)
    {
        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        Draw(surface.Canvas, info, variant);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality);
        return data.ToArray();
    }

    /// <summary>
    /// Renders to a PNG byte array (for inline base64 embedding).
    /// </summary>
    public static byte[] RenderToPng(int width, int height, string variant)
    {
        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        Draw(surface.Canvas, info, variant);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
