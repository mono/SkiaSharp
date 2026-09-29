using System;
using System.Linq;
using SkiaSharp;
using SkiaSharpSample.ImagePlaceholders;
using SkiaSharpSample.Samples;
using Xunit;

public class ImagePlaceholderTests
{
    [Fact]
    public void BlurHashHasIndependentBase83Answers()
    {
        Assert.Equal("00TI:j", BlurHashCodec.Encode(new byte[] { 255, 0, 0, 255 }, 1, 1, 4, 1, 1));
        Assert.Equal("000000", BlurHashCodec.Encode(new byte[] { 0, 0, 0, 255 }, 1, 1, 4, 1, 1));
        Assert.Equal("00TSUA", BlurHashCodec.Encode(new byte[] { 255, 255, 255, 255 }, 1, 1, 4, 1, 1));
        var pixels = BlurHashCodec.Decode("00TI:j", 2, 3);
        for (var i = 0; i < pixels.Length; i += 4)
            Assert.Equal(new byte[] { 255, 0, 0, 255 }, pixels.AsSpan(i, 4).ToArray());
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
    public void PackedPixelContractsAndPaddedRows()
    {
        var padded = new byte[] { 255, 0, 0, 255, 11, 22, 33, 44, 0, 0, 255, 255 };
        var packed = new byte[] { 255, 0, 0, 255, 0, 0, 255, 255 };
        Assert.Equal(BlurHashCodec.Encode(packed, 1, 2, 4, 2, 2),
            BlurHashCodec.Encode(padded, 1, 2, 8, 2, 2));
        Assert.Equal(ThumbHashCodec.Encode(packed, 1, 2, 4),
            ThumbHashCodec.Encode(padded, 1, 2, 8));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.Encode(packed, 1, 2, 3));
        Assert.Throws<ArgumentException>(() => ThumbHashCodec.Encode(packed, 1, 2, 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.Encode(packed, 101, 1, 404));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Encode(packed, 1, 2, 4, 0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => BlurHashCodec.Decode("000000", 4001, 4000));
    }

    [Fact]
    public void ThumbHashHasIndependentReferenceVectorsAndAspect()
    {
        // Generated with evanw/thumbhash js/thumbhash.js at its published reference revision.
        const string black = "AAgCBwAAAAAAAAAAAAAAAAAAAAAAAAAA";
        const string transparent = "AAiCBQAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";
        const string landscape = "FfYqDMF4CIeIiIeAeIePiNuICA==";
        const string portrait = "FfYqDEEIh4h4iIgHd4i4j4jQiA==";
        Assert.Equal(black, Convert.ToBase64String(ThumbHashCodec.Encode(new byte[] { 0, 0, 0, 255 }, 1, 1, 4)));
        Assert.Equal(transparent, Convert.ToBase64String(ThumbHashCodec.Encode(new byte[] { 0, 0, 0, 0 }, 1, 1, 4)));
        var redBlue = new byte[] { 255, 0, 0, 255, 0, 0, 255, 255 };
        Assert.Equal(landscape, Convert.ToBase64String(ThumbHashCodec.Encode(redBlue, 2, 1, 8)));
        Assert.Equal(portrait, Convert.ToBase64String(ThumbHashCodec.Encode(redBlue, 1, 2, 4)));
        using var horizontal = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(landscape));
        using var vertical = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(portrait));
        Assert.True(horizontal.Width > horizontal.Height);
        Assert.True(vertical.Height > vertical.Width);
        using var clear = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(transparent));
        Assert.Equal((byte)0, clear.GetPixel(16, 16).Alpha);
        var rgba = ThumbHashCodec.Decode(Convert.FromBase64String(black), 32, 32, out var w, out var h);
        Assert.Equal((32, 32), (w, h));
        Assert.InRange(rgba[0], (byte)0, (byte)10);
        Assert.Equal((byte)255, rgba[3]);
        var reference = ThumbHashCodec.Decode(Convert.FromBase64String(landscape), 32, 32, out w, out h);
        Assert.Equal((32, 18), (w, h));
        Assert.InRange(reference[0], (byte)35, (byte)55);
        Assert.Equal((byte)255, reference[3]);
        var partial = ThumbHashCodec.Decode(Convert.FromBase64String("5sfOXRqZdgmGeIeYB4ffyPiJiQamWIeYBQ=="), 32, 32, out _, out _);
        Assert.True(partial.Where((_, i) => i % 4 == 3).Distinct().Count() > 1);
    }

    [Theory]
    [InlineData("934A062D069256C374055867DA8AB6679490510719", 23, 32, 46, 160, 94, 6, 255)]
    [InlineData("A1198A1C02383A25D727F68B971FF7F9717F80376758987906", 26, 32, 51, 203, 210, 213, 255)]
    public void ThumbHashDecodesPublishedFlowerAndTux(
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
        using var preview = ThumbHashCodec.DecodeBitmap(bytes);
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
    public void ThumbHashRejectsMalformedBinaryAndInvalidBase64()
    {
        const string black = "AAgCBwAAAAAAAAAAAAAAAAAAAAAAAAAA";
        Assert.Throws<FormatException>(() => Convert.FromBase64String("!!"));
        Assert.Throws<FormatException>(() => Convert.FromBase64String("A"));
        Assert.Throws<FormatException>(() => ThumbHashCodec.DecodeBitmap(new byte[5]));
        var bytes = Convert.FromBase64String(black);
        Assert.Throws<FormatException>(() => ThumbHashCodec.Decode(bytes.AsSpan(0, bytes.Length - 1), 64, 64, out _, out _));
        bytes[3] = 0;
        Assert.Throws<FormatException>(() => ThumbHashCodec.Decode(bytes, 64, 64, out _, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(black), 1001, 1001));
    }

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
        using var decoded = BlurHashCodec.DecodeBitmap("00TI:j");
        Assert.Equal(SKAlphaType.Unpremul, decoded.AlphaType);
    }

    [Fact]
    public void ThumbHashRetainsAlphaAndBoundsPortraitInputs()
    {
        using var source = new SKBitmap(20, 80);
        source.Erase(SKColors.Transparent);
        source.SetPixel(0, 0, SKColors.Red);
        var encoded = ThumbHashCodec.Encode(source);
        using var decoded = ThumbHashCodec.DecodeBitmap(encoded, 24, 64);
        Assert.True(decoded.Height > decoded.Width);
        Assert.InRange(decoded.Width, 1, 24);
        Assert.InRange(decoded.Height, 1, 64);
        Assert.True(decoded.GetPixel(decoded.Width / 2, decoded.Height / 2).Alpha < 255);
        using var blur = BlurHashCodec.DecodeBitmap(BlurHashCodec.Encode(source, 1, 1));
        Assert.Equal((byte)255, blur.GetPixel(32, 32).Alpha);
        Assert.InRange((int)blur.GetPixel(32, 32).Red, 240, 255);
        using var extreme = new SKBitmap(1, 100);
        extreme.Erase(SKColors.Blue);
        using var extremePreview = ThumbHashCodec.DecodeBitmap(ThumbHashCodec.Encode(extreme));
        Assert.InRange(extremePreview.Width, 1, 13);
        Assert.Equal(64, extremePreview.Height);
    }

    [Fact]
    public void TransitionIsDeterministicAcrossBoundariesAndLateStart()
    {
        var start = TimeSpan.FromSeconds(30);
        Assert.Equal(0, PlaceholderTransition.FullOpacity(TimeSpan.FromSeconds(20), start));
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(2), start));
        Assert.InRange(PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(2500), start), 0.49f, 0.51f);
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(3), start));
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(4999), start));
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(5), start));
        Assert.InRange(PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(5500), start), 0.49f, 0.51f);
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(6), start));
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(1_000_008), start));
    }

    [Fact]
    public void ViewRendersInsideAnyRectangleAndRetainsHashOnly()
    {
        using var view = new PlaceholderImageView();
        using var placeholderBitmap = Solid(SKColors.Red);
        using var fullBitmap = Solid(SKColors.Blue);
        var placeholder = SKImage.FromBitmap(placeholderBitmap);
        var full = SKImage.FromBitmap(fullBitmap);
        view.SetPlaceholder(placeholder);
        using var target = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(target);
        canvas.Clear(SKColors.Green);
        var rect = SKRect.Create(25, 30, 40, 20);
        view.Draw(canvas, rect, TimeSpan.FromDays(100));
        Assert.Equal(SKColors.Green, target.GetPixel(0, 0));
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.SetImage(full, TimeSpan.FromSeconds(20));
        view.Draw(canvas, rect, TimeSpan.FromSeconds(22.5));
        Assert.NotEqual(IntPtr.Zero, placeholder.Handle);
        Assert.NotEqual(IntPtr.Zero, full.Handle);
        var mixed = target.GetPixel(45, 40);
        Assert.InRange((int)mixed.Red, 115, 140);
        Assert.InRange((int)mixed.Blue, 115, 140);
        view.Draw(canvas, rect, TimeSpan.FromSeconds(23));
        Assert.Equal(SKColors.Blue, target.GetPixel(45, 40));
        view.Draw(canvas, rect, TimeSpan.FromSeconds(25.5));
        mixed = target.GetPixel(45, 40);
        Assert.InRange((int)mixed.Red, 115, 140);
        Assert.InRange((int)mixed.Blue, 115, 140);
        view.Draw(canvas, rect, TimeSpan.FromSeconds(26));
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.DrawFullImage(canvas, rect);
        Assert.Equal(SKColors.Blue, target.GetPixel(45, 40));
        view.DrawPlaceholder(canvas, rect);
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.Clear();
        Assert.Equal(IntPtr.Zero, placeholder.Handle);
        Assert.Equal(IntPtr.Zero, full.Handle);
        Assert.Equal(SKColors.Green, target.GetPixel(0, 0));
    }

    [Fact]
    public void ViewTransfersEachUniqueImageExactlyOnce()
    {
        using var view = new PlaceholderImageView();
        using var bitmap = Solid(SKColors.Red);
        var first = SKImage.FromBitmap(bitmap);
        var second = SKImage.FromBitmap(bitmap);
        view.SetPlaceholder(first);
        view.SetImage(first, TimeSpan.Zero);
        view.SetPlaceholder(second);
        Assert.NotEqual(IntPtr.Zero, first.Handle);
        view.SetImage(second, TimeSpan.Zero);
        Assert.Equal(IntPtr.Zero, first.Handle);
        view.Dispose();
        view.Dispose();
        Assert.Equal(IntPtr.Zero, second.Handle);
        Assert.Throws<ObjectDisposedException>(() => view.SetImage(null, TimeSpan.Zero));
    }

    [Fact]
    public void ReplacingImageCanKeepTheSharedTransitionPhase()
    {
        using var view = new PlaceholderImageView();
        using var bitmap = Solid(SKColors.Red);
        using var firstBitmap = Solid(SKColors.Blue);
        using var nextBitmap = Solid(SKColors.Green);
        view.SetPlaceholder(SKImage.FromBitmap(bitmap));
        var first = SKImage.FromBitmap(firstBitmap);
        view.SetImage(first, TimeSpan.Zero);
        using var target = Solid(SKColors.White);
        using var canvas = new SKCanvas(target);
        var bounds = SKRect.Create(0, 0, 2, 2);
        view.Draw(canvas, bounds, TimeSpan.FromSeconds(3));
        Assert.Equal(SKColors.Blue, target.GetPixel(1, 1));
        view.SetImage(SKImage.FromBitmap(nextBitmap), TimeSpan.Zero);
        Assert.Equal(IntPtr.Zero, first.Handle);
        view.Draw(canvas, bounds, TimeSpan.FromSeconds(3));
        Assert.Equal(SKColors.Green, target.GetPixel(1, 1));
    }

    private static SKBitmap Solid(SKColor color)
    {
        var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(color);
        return bitmap;
    }

}
