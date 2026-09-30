using System;
using SkiaSharp;

namespace SkiaSharpSample.ImagePlaceholders;

internal static class PixelBuffers
{
    public static void ValidateDimensions(int width, int height, int maxPixels)
    {
        if (width <= 0 || height <= 0 || (long)width * height > maxPixels)
            throw new ArgumentOutOfRangeException(nameof(width), $"Dimensions must be positive and contain no more than {maxPixels} pixels.");
    }

    public static void Validate(ReadOnlySpan<byte> pixels, int width, int height, int stride, int maxPixels)
    {
        ValidateDimensions(width, height, maxPixels);
        if (stride < (long)width * 4 || (long)(height - 1) * stride + (long)width * 4 != pixels.Length)
            throw new ArgumentException("Expected row-major, unpremultiplied RGBA8 bytes with the specified stride.", nameof(pixels));
    }

    public static (byte[] Pixels, int Width, int Height) FromBitmap(SKBitmap source, int maxDimension, bool compositeWhite)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var normalized = Normalize(source, maxDimension, compositeWhite);
        var pixels = new byte[checked(normalized.Width * normalized.Height * 4)];
        var native = Pixels(normalized);
        for (var y = 0; y < normalized.Height; y++)
            native.Slice(y * normalized.RowBytes, normalized.Width * 4).CopyTo(pixels.AsSpan(y * normalized.Width * 4));
        return (pixels, normalized.Width, normalized.Height);
    }

    public static SKBitmap Normalize(SKBitmap source, int maxDimension, bool compositeWhite)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateDimensions(source.Width, source.Height, 16_000_000);
        using var image = SKImage.FromBitmap(source) ?? throw new InvalidOperationException("Unable to read the source bitmap.");
        return Normalize(image, maxDimension, compositeWhite);
    }

    public static SKBitmap Normalize(SKPixmap source, int maxDimension, bool compositeWhite)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateDimensions(source.Width, source.Height, 16_000_000);
        if (source.GetPixels() == IntPtr.Zero)
            throw new ArgumentException("The source pixmap has no pixel data.", nameof(source));
        using var image = SKImage.FromPixels(source) ?? throw new InvalidOperationException("Unable to read the source pixmap.");
        try
        {
            return Normalize(image, maxDimension, compositeWhite);
        }
        finally
        {
            GC.KeepAlive(source);
        }
    }

    public static SKBitmap Normalize(SKImage source, int maxDimension, bool compositeWhite)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateDimensions(source.Width, source.Height, 16_000_000);
        var factor = Math.Min(1, maxDimension / (double)Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * factor, MidpointRounding.AwayFromZero));
        var height = Math.Max(1, (int)Math.Round(source.Height * factor, MidpointRounding.AwayFromZero));
        using var srgb = SKColorSpace.CreateSrgb();
        var normalized = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul, srgb));
        try
        {
            using var canvas = new SKCanvas(normalized);
            canvas.Clear(compositeWhite ? SKColors.White : SKColors.Transparent);
            canvas.DrawImage(source, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear));
            canvas.Flush();
            if (normalized.GetPixels() == IntPtr.Zero)
                throw new InvalidOperationException("Unable to read the normalized bitmap.");
            return normalized;
        }
        catch
        {
            normalized.Dispose();
            throw;
        }
    }

    public static ReadOnlySpan<byte> Pixels(SKBitmap bitmap)
    {
        var length = checked((bitmap.Height - 1) * bitmap.RowBytes + bitmap.Width * 4);
        var pixels = bitmap.GetPixelSpan();
        if (pixels.Length < length)
            throw new InvalidOperationException("Unable to read the normalized bitmap.");
        return pixels[..length];
    }

    public static Span<byte> Destination(SKPixmap pixmap, int width, int height, int maxPixels)
    {
        ArgumentNullException.ThrowIfNull(pixmap);
        ValidateDimensions(width, height, maxPixels);
        if (pixmap.Width != width || pixmap.Height != height ||
            pixmap.ColorType != SKColorType.Rgba8888 || pixmap.AlphaType != SKAlphaType.Unpremul ||
            pixmap.RowBytes < (long)width * 4)
            throw new ArgumentException("Destination must be an unpremultiplied RGBA8888 pixmap of the decoded size.", nameof(pixmap));
        var required = checked((height - 1) * pixmap.RowBytes + width * 4);
        var span = pixmap.GetPixelSpan();
        if (span.Length < required)
            throw new ArgumentException("Destination pixmap has insufficient pixel storage.", nameof(pixmap));
        return span[..required];
    }

    public static void ValidateDestination(Span<byte> destination, int width, int height, int stride, int maxPixels)
    {
        ValidateDimensions(width, height, maxPixels);
        if (stride < (long)width * 4 || destination.Length < (long)(height - 1) * stride + (long)width * 4)
            throw new ArgumentException($"Destination of {destination.Length} bytes must hold {width} by {height} RGBA8 pixels at stride {stride}.", nameof(destination));
    }

}
