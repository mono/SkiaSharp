using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.ImagePlaceholders;
using Xunit;

public class BlurHashTests
{
    [Fact]
    public void BlurHashHasIndependentBase83Answers()
    {
        Assert.Equal("00TI:j", BlurHashCodec.Encode(new byte[] { 255, 0, 0, 255 }, 1, 1, 4, 1, 1));
        Assert.Equal("000000", BlurHashCodec.Encode(new byte[] { 0, 0, 0, 255 }, 1, 1, 4, 1, 1));
        Assert.Equal("00TSUA", BlurHashCodec.Encode(new byte[] { 255, 255, 255, 255 }, 1, 1, 4, 1, 1));
        var pixels = BlurHashCodec.Decode("00TI:j", 2, 3);
        for (var i = 0; i < pixels.Length; i += 4)
        {
            Assert.Equal(new byte[] { 255, 0, 0, 255 }, pixels.AsSpan(i, 4).ToArray());
        }
    }

    [Fact]
    public void BlurHashRequiresComponentsAndDecodeDimensionsButDefaultsPunch()
    {
        const int componentsX = 4;
        const int componentsY = 3;
        const float punch = 1f;
        var pixels = new byte[] { 255, 0, 0, 255 };
        var hash = BlurHashCodec.Encode(pixels, 1, 1, 4, componentsX, componentsY);
        Assert.Equal(BlurHashCodec.Decode(hash, 2, 3, punch),
            BlurHashCodec.Decode(hash, 2, 3));
        using var source = new SKBitmap(1, 1);
        source.Erase(SKColors.Red);
        Assert.Equal(hash, BlurHashCodec.Encode(source, componentsX, componentsY));
        using var preview = BlurHashCodec.DecodeBitmap(hash, 2, 3);
        Assert.Equal((2, 3), (preview.Width, preview.Height));
        var encode = typeof(BlurHashCodec).GetMethod(nameof(BlurHashCodec.Encode),
            [typeof(SKBitmap), typeof(int), typeof(int)])!;
        Assert.False(encode.GetParameters()[1].IsOptional);
        Assert.False(encode.GetParameters()[2].IsOptional);
        var decode = typeof(BlurHashCodec).GetMethod(nameof(BlurHashCodec.DecodeBitmap),
            [typeof(string), typeof(int), typeof(int), typeof(float)])!;
        Assert.Equal(punch, decode.GetParameters()[3].DefaultValue);
        Assert.DoesNotContain(typeof(BlurHashCodec).GetMethods(),
            method => method.Name == nameof(BlurHashCodec.DecodeBitmap) && method.GetParameters().Length == 1);
    }

    [Fact]
    public void BlurHashDecodesPublishedExampleAndValidatesEveryField()
    {
        var rgba = BlurHashCodec.Decode("LlMF%n00%#MwS|WCWEM{R*bbWBbH", 10, 10);
        Assert.Equal(400, rgba.Length);
        Assert.All(rgba.Where((_, i) => i % 4 == 3), alpha => Assert.Equal((byte)255, alpha));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("!", 4, 4));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("0!0000", 4, 4));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("00####", 4, 4));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("00TI:j ", 4, 4));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("100000~~", 4, 4));
        Assert.Throws<FormatException>(() => BlurHashCodec.Decode("~0" + new string('0', 42), 4, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode("00TI:j", 4, 0));
    }

    [Fact]
    public void BlurHashPunchChangesOnlyTheDecodedColorVariation()
    {
        var source = new byte[] { 80, 80, 80, 255, 160, 160, 160, 255 };
        var hash = BlurHashCodec.Encode(source, 2, 1, 8, 2, 1);
        var original = BlurHashCodec.Decode(hash, 2, 1);
        Assert.Equal(original, BlurHashCodec.Decode(hash, 2, 1, 1f));

        var flat = BlurHashCodec.Decode(hash, 2, 1, 0f);
        Assert.Equal(flat.AsSpan(0, 4).ToArray(), flat.AsSpan(4, 4).ToArray());
        var stronger = BlurHashCodec.Decode(hash, 2, 1, 2f);
        Assert.True(Math.Abs(stronger[0] - stronger[4]) > Math.Abs(original[0] - original[4]));
        using var bitmap = BlurHashCodec.DecodeBitmap(hash, 2, 1, 2f);
        Assert.Equal(stronger[0], bitmap.GetPixel(0, 0).Red);

        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode(hash, 2, 1, -1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode(hash, 2, 1, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode(hash, 2, 1, float.PositiveInfinity));
    }

    [Theory]
    [InlineData(1, 9)]
    [InlineData(9, 1)]
    [InlineData(9, 9)]
    public void BlurHashSupportsExtremeComponentCounts(int componentsX, int componentsY)
    {
        var grayscale = new byte[] { 32, 32, 32, 255, 224, 224, 224, 255 };
        var encoded = BlurHashCodec.Encode(grayscale, 2, 1, 8, componentsX, componentsY);
        Assert.Equal(4 + 2 * componentsX * componentsY, encoded.Length);
        Assert.Equal(encoded, BlurHashCodec.Encode(grayscale, 2, 1, 8, componentsX, componentsY));
        Assert.Equal(2 * 3 * 4, BlurHashCodec.Decode(encoded, 2, 3).Length);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BlurHashCodec.Encode(grayscale, 2, 1, 8, componentsX + 9, componentsY));
    }

    [Fact]
    public void BlurHashPackedPixelContractsAndPaddedRows()
    {
        var padded = new byte[] { 255, 0, 0, 255, 11, 22, 33, 44, 0, 0, 255, 255 };
        var packed = new byte[] { 255, 0, 0, 255, 0, 0, 255, 255 };
        Assert.Equal(BlurHashCodec.Encode(packed, 1, 2, 4, 2, 2),
            BlurHashCodec.Encode(padded, 1, 2, 8, 2, 2));
        Assert.Throws<ArgumentException>(() => BlurHashCodec.Encode((byte[])null!, 1, 1, 4, 1, 1));
        Assert.Throws<ArgumentNullException>(() => BlurHashCodec.Encode((SKImage)null!, 1, 1));
        Assert.Throws<ArgumentNullException>(() => BlurHashCodec.Decode((string)null!, 4, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Encode(packed, 1, 2, 4, 0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode("000000", 4001, 4000));
    }

    [Fact]
    public void AllSkiaSourceTypesProduceTheSameHashesWithoutTakingOwnership()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Bgra8888, SKAlphaType.Premul), 32);
        bitmap.Erase(SKColors.Transparent);
        bitmap.SetPixel(0, 0, SKColors.Red);
        bitmap.SetPixel(1, 0, new SKColor(25, 70, 180, 125));
        using var image = SKImage.FromBitmap(bitmap);
        using var pixmap = bitmap.PeekPixels();
        Assert.Equal(BlurHashCodec.Encode(bitmap, 4, 3), BlurHashCodec.Encode(image, 4, 3));
        Assert.Equal(BlurHashCodec.Encode(bitmap, 2, 3), BlurHashCodec.Encode(pixmap, 2, 3));
        using var normalized = PixelBuffers.Normalize(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        Assert.Equal(BlurHashCodec.Encode(bitmap, 4, 3),
            BlurHashCodec.Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, 4, 3));
        var (pixels, width, height) = PixelBuffers.FromBitmap(bitmap, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        Assert.Equal(BlurHashCodec.Encode(bitmap, 4, 3),
            BlurHashCodec.Encode(pixels, width, height, width * PixelBuffers.RgbaBytesPerPixel, 4, 3));
        Assert.NotEqual(IntPtr.Zero, bitmap.Handle);
        Assert.NotEqual(IntPtr.Zero, image.Handle);
        Assert.NotEqual(IntPtr.Zero, pixmap.Handle);
    }

    [Fact]
    public void RasterImagePeekPathMatchesNormalizedPixelsWithoutTakingOwnership()
    {
        using var srgb = SKColorSpace.CreateSrgb();
        using var opaque = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Rgba8888, SKAlphaType.Opaque, srgb), 20);
        opaque.Erase(SKColors.Red);
        opaque.SetPixel(1, 0, SKColors.Blue);
        using var image = SKImage.FromBitmap(opaque);
        using var peek = PixelBuffers.PeekEncodingPixels(image, compositeWhite: true);
        Assert.NotNull(peek);
        Assert.Equal(20, peek.RowBytes);
        using var normalized = PixelBuffers.Normalize(image, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        Assert.Equal(BlurHashCodec.Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, 3, 2),
            BlurHashCodec.Encode(image, 3, 2));

        Assert.Null(PixelBuffers.PeekEncodingPixels(image, compositeWhite: false));
        Assert.NotEqual(IntPtr.Zero, image.Handle);
    }

    [Theory]
    [InlineData(16, 12)]
    [InlineData(3, 19)]
    public void ImagePeekOrFallbackPreservesEncodedHashes(int width, int height)
    {
        using var srgb = SKColorSpace.CreateSrgb();
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul, srgb),
            width * 4 + 12);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor((byte)(x * 37 + y * 11),
                    (byte)(y * 29 + x * 3), (byte)(x * 7 + y * 17), (byte)((x * 19 + y * 41) % 256)));
            }
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var opaque = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque, srgb));
        opaque.Erase(SKColors.Blue);
        using var raster = SKImage.FromBitmap(opaque);
        using var encoded = opaque.Encode(SKEncodedImageFormat.Png, 100);
        using var lazy = SKImage.FromEncodedData(encoded);
        Assert.Null(PixelBuffers.PeekEncodingPixels(lazy, compositeWhite: true));
        Assert.Equal(BlurHashCodec.Encode(opaque, 3, 2), BlurHashCodec.Encode(raster, 3, 2));
        Assert.Equal(BlurHashCodec.Encode(opaque, 3, 2), BlurHashCodec.Encode(lazy, 3, 2));
    }

    [Fact]
    public void BlurHashWritesCallerOwnedPaddedSpansAndPixmap()
    {
        const string hash = "LEHV6nWB2yk8pyo0adR*.7kCMdnj";
        var packed = BlurHashCodec.Decode(hash, 3, 2, 1.5f);
        Assert.Equal(BlurHashCodec.Decode(hash, 3, 2), BlurHashCodec.Decode(hash.AsSpan(), 3, 2));
        var padded = Enumerable.Repeat((byte)17, 32).ToArray();
        BlurHashCodec.DecodeInto(hash.AsSpan(), padded, 3, 2, 20, 1.5f);
        Assert.Equal(packed.AsSpan(0, 12).ToArray(), padded.AsSpan(0, 12).ToArray());
        Assert.Equal(packed.AsSpan(12, 12).ToArray(), padded.AsSpan(20, 12).ToArray());
        Assert.All(padded.AsSpan(12, 8).ToArray(), value => Assert.Equal((byte)17, value));
        using var bitmap = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Rgba8888, SKAlphaType.Unpremul), 20);
        using var pixmap = bitmap.PeekPixels();
        BlurHashCodec.DecodeInto(hash, pixmap, 1.5f);
        Assert.Equal(packed.AsSpan(12, 12).ToArray(), pixmap.GetPixelSpan().Slice(20, 12).ToArray());
        using var decoded = BlurHashCodec.DecodeBitmap(hash, 3, 2, 1.5f);
        using var image = BlurHashCodec.DecodeImage(hash, 3, 2, 1.5f);
        using var imagePixels = SKBitmap.FromImage(image);
        Assert.Equal(decoded.GetPixel(1, 1), bitmap.GetPixel(1, 1));
        Assert.Equal(decoded.GetPixel(1, 1), imagePixels.GetPixel(1, 1));
        Assert.Equal(3, image.Width);
        Assert.Throws<ArgumentException>(() => BlurHashCodec.DecodeInto(hash, new byte[24], 3, 2, 20, 1f));
        Assert.Throws<FormatException>(() => BlurHashCodec.DecodeInto("!", pixmap, 1f));
        using var wrong = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Bgra8888, SKAlphaType.Premul));
        using var wrongPixmap = wrong.PeekPixels();
        Assert.Throws<ArgumentException>(() => BlurHashCodec.DecodeInto(hash, wrongPixmap, 1f));
    }

    [Fact]
    public void PooledScratchDoesNotCrossContaminateConcurrentCalls()
    {
        var pixels = new byte[] { 10, 30, 50, 255, 230, 120, 80, 128 };
        var blurHash = BlurHashCodec.Encode(pixels, 2, 1, 8, 4, 3);
        var blurPreview = BlurHashCodec.Decode(blurHash, 7, 5);
        Parallel.For(0, 16, iteration =>
        {
            Assert.Equal(blurHash, BlurHashCodec.Encode(pixels, 2, 1, 8, 4, 3));
            Assert.Equal(blurPreview, BlurHashCodec.Decode(blurHash, 7, 5));
        });
    }

    [Fact]
    public void ExtremelyWideLowLevelPreviewsDoNotRequireProportionalCosineCaches()
    {
        var pixels = BlurHashCodec.Decode("00TI:j", 70_000, 1);
        Assert.Equal(70_000 * 4, pixels.Length);
        Assert.Equal("00TI:j", BlurHashCodec.Encode(pixels, 70_000, 1, 70_000 * 4, 1, 1));
    }

}
