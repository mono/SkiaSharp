using SkiaSharp;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Standard test image used across all integration tests.
/// This draws a consistent pattern that can be verified via screenshot comparison.
/// </summary>
public static class TestImage
{
    public const int Width = 400;
    public const int Height = 300;

    /// <summary>
    /// Draws the standard test image on a canvas.
    /// This exact code is embedded in test apps to produce consistent output.
    /// </summary>
    public static void Draw(SKCanvas canvas)
    {
        // Light gray background
        canvas.Clear(new SKColor(240, 240, 240));

        // Blue circle in center
        using (var paint = new SKPaint { Color = SKColors.Blue, IsAntialias = true })
        {
            canvas.DrawCircle(Width / 2f, Height / 2f, 80, paint);
        }

        // Green rectangle in bottom-right
        using (var paint = new SKPaint { Color = new SKColor(0, 150, 80), IsAntialias = true })
        {
            canvas.DrawRect(280, 180, 100, 100, paint);
        }

        // Red text at top
        using var font = new SKFont(SKTypeface.Default, 24);
        using (var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true })
        {
            canvas.DrawText("SkiaSharp Test", 20, 40, SKTextAlign.Left, font, paint);
        }
    }

    /// <summary>
    /// Gets the C# code to embed in test apps that draws this test image.
    /// </summary>
    public static string GetDrawCode() => """
        canvas.Clear(new SKColor(240, 240, 240));

        using (var paint = new SKPaint { Color = SKColors.Blue, IsAntialias = true })
            canvas.DrawCircle(200, 150, 80, paint);

        using (var paint = new SKPaint { Color = new SKColor(0, 150, 80), IsAntialias = true })
            canvas.DrawRect(280, 180, 100, 100, paint);

        using var font = new SKFont(SKTypeface.Default, 24);
        using (var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true })
            canvas.DrawText("SkiaSharp Test", 20, 40, SKTextAlign.Left, font, paint);
        """;

    /// <summary>
    /// Renders the test image to a bitmap.
    /// </summary>
    public static SKBitmap Render()
    {
        var bitmap = new SKBitmap(Width, Height);
        using var canvas = new SKCanvas(bitmap);
        Draw(canvas);
        return bitmap;
    }

    /// <summary>
    /// Gets the embedded reference image from assembly resources.
    /// </summary>
    /// <param name="platform">Optional platform suffix (e.g., "maccatalyst", "ios", "android")</param>
    public static byte[] GetReferenceImage(string? platform = null)
    {
        var assembly = typeof(TestImage).Assembly;
        var suffix = string.IsNullOrEmpty(platform) ? "" : $"-{platform}";
        var targetName = $"reference-test-image{suffix}.png";

        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(targetName));
        
        // Fall back to base reference if platform-specific not found
        if (resourceName == null && !string.IsNullOrEmpty(platform))
        {
            resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("reference-test-image.png"));
        }
        
        _ = resourceName ?? throw new Exception($"Reference image resource not found: {targetName}");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        _ = stream ?? throw new Exception("Reference image resource stream not found");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
