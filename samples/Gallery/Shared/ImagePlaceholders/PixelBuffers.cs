using System;
using System.Runtime.InteropServices;
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
        ValidateDimensions(source.Width, source.Height, 16_000_000);
        var factor = Math.Min(1, maxDimension / (double)Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * factor, MidpointRounding.AwayFromZero));
        var height = Math.Max(1, (int)Math.Round(source.Height * factor, MidpointRounding.AwayFromZero));
        using var srgb = SKColorSpace.CreateSrgb();
        using var normalized = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul, srgb));
        using (var canvas = new SKCanvas(normalized))
        {
            canvas.Clear(compositeWhite ? SKColors.White : SKColors.Transparent);
            canvas.DrawBitmap(source, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear));
            canvas.Flush();
        }
        var bytes = new byte[width * height * 4];
        var ptr = normalized.GetPixels();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Unable to read the normalized bitmap.");
        for (var y = 0; y < height; y++)
            Marshal.Copy(IntPtr.Add(ptr, y * normalized.RowBytes), bytes, y * width * 4, width * 4);
        return (bytes, width, height);
    }

    public static SKBitmap ToBitmap(byte[] rgba, int width, int height)
    {
        Validate(rgba, width, height, width * 4, 16_000_000);
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        try
        {
            var ptr = bitmap.GetPixels();
            if (ptr == IntPtr.Zero)
                throw new InvalidOperationException("Unable to allocate the decoded bitmap.");
            for (var y = 0; y < height; y++)
                Marshal.Copy(rgba, y * width * 4, IntPtr.Add(ptr, y * bitmap.RowBytes), width * 4);
            bitmap.NotifyPixelsChanged();
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}
