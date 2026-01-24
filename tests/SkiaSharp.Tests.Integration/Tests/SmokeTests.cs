using SkiaSharp;
using Xunit;
using Xunit.Abstractions;
using HBuffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Smoke tests to verify native libraries load correctly.
/// These are quick sanity checks - the real tests are the platform app builds.
/// </summary>
public class SmokeTests(ITestOutputHelper output)
{
    [Fact]
    public void SkiaSharpNativeLoads()
    {
        // This will throw if native library fails to load
        using var bitmap = new SKBitmap(100, 100);
        
        Assert.NotNull(bitmap);
        Assert.NotEqual(IntPtr.Zero, bitmap.GetPixels());
        
        var version = typeof(SKBitmap).Assembly.GetName().Version;
        output.WriteLine($"✅ SkiaSharp {version} native library loaded");
    }

    [Fact]
    public void HarfBuzzSharpNativeLoads()
    {
        // This will throw if native library fails to load
        using var buffer = new HBuffer();
        buffer.AddUtf8("Test");
        
        Assert.True(buffer.Length > 0);
        
        var version = typeof(HBuffer).Assembly.GetName().Version;
        output.WriteLine($"✅ HarfBuzzSharp {version} native library loaded");
    }

    [Fact]
    public void CanRenderAndEncode()
    {
        // Quick end-to-end: create bitmap, draw, encode to PNG
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        
        canvas.Clear(SKColors.Blue);
        using var paint = new SKPaint { Color = SKColors.Red };
        canvas.DrawCircle(50, 50, 40, paint);
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        Assert.True(data.Size > 0);
        output.WriteLine($"✅ Rendered and encoded PNG ({data.Size} bytes)");
    }
}
