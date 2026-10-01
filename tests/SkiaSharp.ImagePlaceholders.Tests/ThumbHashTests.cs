using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.ImagePlaceholders;
using Xunit;

public class ThumbHashTests
{
    // Encoded with evanw/thumbhash js/thumbhash.js at its published reference revision.
    private const string Black = "AAgCBwAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string Transparent = "AAiCBQAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";
    private const string Landscape = "FfYqDMF4CIeIiIeAeIePiNuICA==";
    private const string Portrait = "FfYqDEEIh4h4iIgHd4i4j4jQiA==";

    [Fact]
    public void EncodesIndependentReferenceVectorsAndPreservesAspectAndAlpha()
    {
        Assert.Equal(Black, Convert.ToBase64String(ThumbHashCodec.Encode(new byte[] { 0, 0, 0, 255 }, 1, 1, 4)));
        Assert.Equal(Transparent, Convert.ToBase64String(ThumbHashCodec.Encode(new byte[] { 0, 0, 0, 0 }, 1, 1, 4)));
        var redBlue = new byte[] { 255, 0, 0, 255, 0, 0, 255, 255 };
        Assert.Equal(Landscape, Convert.ToBase64String(ThumbHashCodec.Encode(redBlue, 2, 1, 8)));
        Assert.Equal(Portrait, Convert.ToBase64String(ThumbHashCodec.Encode(redBlue, 1, 2, 4)));

        using var horizontal = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(Landscape), 64, 64);
        using var vertical = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(Portrait), 64, 64);
        Assert.True(horizontal.Width > horizontal.Height);
        Assert.True(vertical.Height > vertical.Width);
        using var clear = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(Transparent), 64, 64);
        Assert.Equal((byte)0, clear.GetPixel(16, 16).Alpha);
        var rgba = ThumbHashCodec.Decode(Convert.FromBase64String(Black), 32, 32, out var width, out var height);
        Assert.Equal((32, 32), (width, height));
        Assert.InRange(rgba[0], (byte)0, (byte)10);
        Assert.Equal((byte)255, rgba[3]);
        var reference = ThumbHashCodec.Decode(Convert.FromBase64String(Landscape), 32, 32, out width, out height);
        Assert.Equal((32, 18), (width, height));
        Assert.InRange(reference[0], (byte)35, (byte)55);
        Assert.Equal((byte)255, reference[3]);
        var partial = ThumbHashCodec.Decode(Convert.FromBase64String("5sfOXRqZdgmGeIeYB4ffyPiJiQamWIeYBQ=="), 32, 32, out _, out _);
        Assert.True(partial.Where((_, i) => i % 4 == 3).Distinct().Count() > 1);
    }

    [Theory]
    [InlineData("934A062D069256C374055867DA8AB6679490510719", 23, 32, 46, 160, 94, 6, 255)]
    [InlineData("A1198A1C02383A25D727F68B971FF7F9717F80376758987906", 26, 32, 51, 203, 210, 213, 255)]
    public void DecodesPublishedFlowerAndTux(
        string hex, int expectedWidth, int expectedHeight, int previewWidth, int red, int green, int blue, int alpha)
    {
        // Independent vectors: jzebedee/ThumbHash master README (flower.jpg and tux.png).
        // Expected sizes and center pixels checked against evanw/thumbhash js/thumbhash.js.
        var bytes = Convert.FromHexString(hex);
        var pixels = ThumbHashCodec.Decode(bytes, 32, 32, out var width, out var height);
        Assert.Equal((expectedWidth, expectedHeight), (width, height));
        var middle = ((height / 2) * width + width / 2) * 4;
        Assert.InRange((int)pixels[middle], red - 8, red + 8);
        Assert.InRange((int)pixels[middle + 1], green - 8, green + 8);
        Assert.InRange((int)pixels[middle + 2], Math.Max(0, blue - 8), blue + 8);
        Assert.InRange((int)pixels[middle + 3], alpha - 8, alpha);
        using var preview = ThumbHashCodec.DecodeBitmap(bytes, 64, 64);
        Assert.Equal((previewWidth, 64), (preview.Width, preview.Height));
        Assert.InRange(Math.Abs(pixels[middle + 3] - preview.GetPixel(preview.Width / 2, preview.Height / 2).Alpha), 0, 8);
        if (hex.StartsWith("A119", StringComparison.Ordinal))
        {
            Assert.Equal((byte)0, pixels[3]);
            Assert.True(pixels.Where((_, i) => i % 4 == 3).Distinct().Count() > 1);
        }
        else
            Assert.All(pixels.Where((_, i) => i % 4 == 3), value => Assert.Equal((byte)255, value));
    }

    [Fact]
    public void RejectsInvalidBase64TruncatedHeadersAndMalformedPayloads()
    {
        Assert.Throws<FormatException>(() => Convert.FromBase64String("!!"));
        Assert.Throws<FormatException>(() => Convert.FromBase64String("A"));
        Assert.Throws<FormatException>(() => ThumbHashCodec.GetDecodedSize(Array.Empty<byte>(), 64, 64));
        Assert.Throws<FormatException>(() => ThumbHashCodec.DecodeBitmap(new byte[5], 64, 64));
        var valid = Convert.FromBase64String(Black);
        Assert.Throws<FormatException>(() => ThumbHashCodec.Decode(valid.AsSpan(0, valid.Length - 1), 64, 64, out _, out _));
        Assert.Throws<FormatException>(() => ThumbHashCodec.GetDecodedSize(valid.Concat(new byte[1]).ToArray(), 64, 64));
        var invalidPadding = (byte[])valid.Clone();
        invalidPadding[^1] |= 0xf0;
        Assert.Throws<FormatException>(() => ThumbHashCodec.GetDecodedSize(invalidPadding, 64, 64));
        valid[3] = 0;
        Assert.Throws<FormatException>(() => ThumbHashCodec.Decode(valid, 64, 64, out _, out _));
        var alpha = Convert.FromBase64String(Transparent);
        Assert.Throws<FormatException>(() => ThumbHashCodec.GetDecodedSize(alpha.AsSpan(0, 5), 64, 64));
        Assert.Throws<FormatException>(() => ThumbHashCodec.GetDecodedSize(alpha.AsSpan(0, alpha.Length - 1), 64, 64));
        Assert.Throws<FormatException>(() => ThumbHashCodec.Decode((byte[])null!, 4, 4, out _, out _));
    }

    [Fact]
    public void ValidatesInputDimensionsStrideAndPreviewBounds()
    {
        var packed = new byte[] { 255, 0, 0, 255, 0, 0, 255, 255 };
        var padded = new byte[] { 255, 0, 0, 255, 11, 22, 33, 44, 0, 0, 255, 255 };
        Assert.Equal(ThumbHashCodec.Encode(packed, 1, 2, 4), ThumbHashCodec.Encode(padded, 1, 2, 8));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.Encode(packed, 1, 2, 3));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.Encode(packed, 1, 2, 8));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.Encode((byte[])null!, 1, 1, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.Encode(packed, 101, 1, 404));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.Encode(packed, 0, 1, 4));
        var maximum = Enumerable.Repeat((byte)255, 100 * 100 * 4).ToArray();
        Assert.NotEmpty(ThumbHashCodec.Encode(maximum, 100, 100, 400));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(Black), 1001, 1001));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.GetDecodedSize(Convert.FromBase64String(Black), 0, 64));
        Assert.DoesNotContain(typeof(ThumbHashCodec).GetMethods(),
            method => method.Name == nameof(ThumbHashCodec.DecodeBitmap) && method.GetParameters().Length == 1);
    }

    [Fact]
    public void NormalizesPortraitAndLargeInputsWithoutDiscardingTransparency()
    {
        using var source = new SKBitmap(20, 80);
        source.Erase(SKColors.Transparent);
        source.SetPixel(0, 0, SKColors.Red);
        using var decoded = ThumbHashCodec.DecodeBitmap(ThumbHashCodec.Encode(source), 24, 64);
        Assert.True(decoded.Height > decoded.Width);
        Assert.InRange(decoded.Width, 1, 24);
        Assert.InRange(decoded.Height, 1, 64);
        Assert.True(decoded.GetPixel(decoded.Width / 2, decoded.Height / 2).Alpha < 255);
        using var extreme = new SKBitmap(1, 100);
        extreme.Erase(SKColors.Blue);
        using var extremePreview = ThumbHashCodec.DecodeBitmap(ThumbHashCodec.Encode(extreme), 64, 64);
        Assert.InRange(extremePreview.Width, 1, 13);
        Assert.Equal(64, extremePreview.Height);
        using var oversized = new SKBitmap(110, 55);
        oversized.Erase(SKColors.Green);
        using var scaled = PixelBuffers.Normalize(oversized, 100, compositeWhite: false);
        Assert.Equal(ThumbHashCodec.Encode(PixelBuffers.Pixels(scaled), scaled.Width, scaled.Height, scaled.RowBytes),
            ThumbHashCodec.Encode(oversized));
    }

    [Fact]
    public void SkiaAdaptersConvertBgraPremultiplicationAndRetainCallerOwnership()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(3, 2, SKColorType.Bgra8888, SKAlphaType.Premul), 32);
        bitmap.Erase(SKColors.Transparent);
        bitmap.SetPixel(0, 0, SKColors.Red);
        bitmap.SetPixel(1, 0, new SKColor(25, 70, 180, 125));
        using var image = SKImage.FromBitmap(bitmap);
        using var pixmap = bitmap.PeekPixels();
        var expected = ThumbHashCodec.Encode(bitmap);
        Assert.Equal(expected, ThumbHashCodec.Encode(image));
        Assert.Equal(expected, ThumbHashCodec.Encode(pixmap));
        using var normalized = PixelBuffers.Normalize(bitmap, 100, compositeWhite: false);
        Assert.Equal(expected, ThumbHashCodec.Encode(PixelBuffers.Pixels(normalized),
            normalized.Width, normalized.Height, normalized.RowBytes));
        Assert.NotEqual(IntPtr.Zero, bitmap.Handle);
        Assert.NotEqual(IntPtr.Zero, image.Handle);
        Assert.NotEqual(IntPtr.Zero, pixmap.Handle);
        Assert.Throws<ArgumentNullException>(() => ThumbHashCodec.Encode((SKBitmap)null!));
        Assert.Throws<ArgumentNullException>(() => ThumbHashCodec.Encode((SKImage)null!));
        Assert.Throws<ArgumentNullException>(() => ThumbHashCodec.Encode((SKPixmap)null!));
    }

    [Theory]
    [InlineData(16, 12)]
    [InlineData(3, 19)]
    [InlineData(101, 50)]
    public void ImagePeekAndFallbackMatchNormalizedEncoding(int width, int height)
    {
        using var srgb = SKColorSpace.CreateSrgb();
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul, srgb),
            width * 4 + 12);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                bitmap.SetPixel(x, y, new SKColor((byte)(x * 37 + y * 11),
                    (byte)(y * 29 + x * 3), (byte)(x * 7 + y * 17), (byte)((x * 19 + y * 41) % 256)));
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var normalized = PixelBuffers.Normalize(image, 100, compositeWhite: false);
        Assert.Equal(ThumbHashCodec.Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes),
            ThumbHashCodec.Encode(image));
        Assert.Equal(ThumbHashCodec.Encode(bitmap), ThumbHashCodec.Encode(image));
        Assert.NotEqual(IntPtr.Zero, image.Handle);

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var lazy = SKImage.FromEncodedData(data);
        Assert.Null(PixelBuffers.PeekEncodingPixels(lazy, compositeWhite: false));
        using var lazyNormalized = PixelBuffers.Normalize(lazy, 100, compositeWhite: false);
        Assert.Equal(ThumbHashCodec.Encode(PixelBuffers.Pixels(lazyNormalized),
            lazyNormalized.Width, lazyNormalized.Height, lazyNormalized.RowBytes), ThumbHashCodec.Encode(lazy));
        Assert.NotEqual(IntPtr.Zero, lazy.Handle);
    }

    [Fact]
    public void DecodesIntoCallerPaddedSpanAndPixmapWithoutTakingOwnership()
    {
        var hash = Convert.FromBase64String(Landscape);
        var packed = ThumbHashCodec.Decode(hash, 4, 4, out var width, out var height);
        Assert.Equal(new SKSizeI(width, height), ThumbHashCodec.GetDecodedSize(hash, 4, 4));
        var stride = width * 4 + 8;
        var padded = Enumerable.Repeat((byte)17, height * stride).ToArray();
        ThumbHashCodec.DecodeInto(hash, padded, 4, 4, stride, out var actualWidth, out var actualHeight);
        Assert.Equal((width, height), (actualWidth, actualHeight));
        for (var y = 0; y < height; y++)
        {
            Assert.Equal(packed.AsSpan(y * width * 4, width * 4).ToArray(),
                padded.AsSpan(y * stride, width * 4).ToArray());
            Assert.All(padded.AsSpan(y * stride + width * 4, 8).ToArray(), value => Assert.Equal((byte)17, value));
        }
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul), stride);
        using var pixmap = bitmap.PeekPixels();
        ThumbHashCodec.DecodeInto(hash, pixmap, 4, 4);
        using var decoded = ThumbHashCodec.DecodeBitmap(hash, 4, 4);
        using var image = ThumbHashCodec.DecodeImage(hash, 4, 4);
        using var imagePixels = SKBitmap.FromImage(image);
        Assert.Equal(decoded.GetPixel(1, 1), bitmap.GetPixel(1, 1));
        Assert.Equal(decoded.GetPixel(1, 1), imagePixels.GetPixel(1, 1));
        Assert.Equal(width, image.Width);
        Assert.NotEqual(IntPtr.Zero, pixmap.Handle);
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.DecodeInto(hash, pixmap, 3, 3));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.DecodeInto(hash, new byte[4], 4, 4, stride, out _, out _));
        Assert.Throws<FormatException>(() => ThumbHashCodec.DecodeInto(new byte[5], padded, 4, 4, stride, out _, out _));
    }

    [Fact]
    public void RepeatedAndConcurrentCallsAreDeterministicAndIndependent()
    {
        var pixels = new byte[] { 10, 30, 50, 255, 230, 120, 80, 128 };
        var hash = ThumbHashCodec.Encode(pixels, 2, 1, 8);
        var preview = ThumbHashCodec.Decode(hash, 7, 5, out _, out _);
        Parallel.For(0, 16, _ =>
        {
            Assert.Equal(hash, ThumbHashCodec.Encode(pixels, 2, 1, 8));
            Assert.Equal(preview, ThumbHashCodec.Decode(hash, 7, 5, out _, out _));
        });
        int aliasedDimension;
        Assert.Equal(preview, ThumbHashCodec.Decode(hash, 7, 5, out aliasedDimension, out aliasedDimension));
        Assert.InRange(aliasedDimension, 1, 7);
    }
}
