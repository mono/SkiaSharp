using System;
using SkiaSharp;
using SkiaSharpSample.ImagePlaceholders;
using Xunit;

public class PixelBuffersTests
{
    [Fact]
    public void BitmapAdapterConvertsAlphaBgraAndSourceStride()
    {
        using var bgra = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Bgra8888, SKAlphaType.Premul), 32);
        bgra.SetPixel(0, 0, new SKColor(255, 0, 0));
        bgra.SetPixel(1, 0, new SKColor(0, 0, 255, 128));
        bgra.SetPixel(2, 1, SKColors.Transparent);
        using var rgba = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        rgba.SetPixel(0, 0, new SKColor(255, 0, 0));
        rgba.SetPixel(1, 0, new SKColor(0, 0, 255, 128));
        rgba.SetPixel(2, 1, SKColors.Transparent);
        var (a, aw, ah) = PixelBuffers.FromBitmap(bgra, 100, true);
        var (b, bw, bh) = PixelBuffers.FromBitmap(rgba, 100, true);
        Assert.Equal((3, 2), (aw, ah));
        Assert.Equal((aw, ah), (bw, bh));
        for (var i = 0; i < a.Length; i++)
            Assert.InRange(Math.Abs(a[i] - b[i]), 0, 2);
        Assert.Equal(new byte[] { 255, 255, 255, 255 }, a.AsSpan(20, 4).ToArray());
        Assert.Equal(255, a[0]);
        var (transparent, _, _) = PixelBuffers.FromBitmap(bgra, 100, false);
        Assert.Equal(0, transparent[23]);
        using var decoded = BlurHashCodec.DecodeBitmap("00TI:j", 64, 64);
        Assert.Equal(SKAlphaType.Unpremul, decoded.AlphaType);
    }

    [Fact]
    public void ValidRasterImageCanBePeekedWithoutAllocatingPixelArrays()
    {
        using var srgb = SKColorSpace.CreateSrgb();
        using var bitmap = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Rgba8888, SKAlphaType.Opaque, srgb), 20);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var peek = PixelBuffers.PeekEncodingPixels(image, compositeWhite: true);
        Assert.NotNull(peek);
        Assert.Equal(20, peek.RowBytes);
        Assert.Equal(32, PixelBuffers.Pixels(peek).Length);
        Assert.Equal(bitmap.GetPixel(0, 0), peek.GetPixelColor(0, 0));
        Assert.NotEqual(IntPtr.Zero, image.Handle);
        _ = PixelBuffers.Pixels(peek)[0];
        var before = GC.GetAllocatedBytesForCurrentThread();
        var checksum = 0;
        for (var i = 0; i < 128; i++)
            checksum += PixelBuffers.Pixels(peek)[0];
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(255 * 128, checksum);

        using var transparent = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Rgba8888, SKAlphaType.Unpremul, srgb));
        transparent.Erase(SKColors.Transparent);
        using var transparentImage = SKImage.FromBitmap(transparent);
        Assert.Null(PixelBuffers.PeekEncodingPixels(transparentImage, compositeWhite: true));
        Assert.Null(PixelBuffers.PeekEncodingPixels(image, compositeWhite: false));
        using var unpremulPeek = PixelBuffers.PeekEncodingPixels(transparentImage, compositeWhite: false);
        Assert.NotNull(unpremulPeek);
    }

    [Fact]
    public void NormalizationCapsThumbnailSizeAndPreservesOpaqueComposite()
    {
        using var bitmap = new SKBitmap(20, 80);
        bitmap.Erase(SKColors.Transparent);
        bitmap.SetPixel(0, 0, SKColors.Red);
        using var normalized = PixelBuffers.Normalize(bitmap, 24, compositeWhite: true);
        Assert.Equal((6, 24), (normalized.Width, normalized.Height));
        Assert.Equal(SKAlphaType.Unpremul, normalized.AlphaType);
        var pixels = PixelBuffers.Pixels(normalized);
        for (var i = 3; i < pixels.Length; i += 4)
            Assert.Equal((byte)255, pixels[i]);
        using var preview = BlurHashCodec.DecodeBitmap(BlurHashCodec.Encode(bitmap, 1, 1), 64, 64);
        Assert.Equal((byte)255, preview.GetPixel(32, 32).Alpha);
        Assert.InRange((int)preview.GetPixel(32, 32).Red, 240, 255);
    }
}
