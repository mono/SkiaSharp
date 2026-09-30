// BlurHash format and reference implementation by Wolt Enterprises:
// https://github.com/woltapp/blurhash
// MIT License
// Copyright (c) 2018 Wolt Enterprises
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Buffers;
using SkiaSharp;

namespace SkiaSharpSample.ImagePlaceholders;

/// <summary>Encodes and decodes standard RGB BlurHash base83 strings.</summary>
/// <remarks>BlurHash has no alpha channel. Bitmap encoding composites transparent pixels over white;
/// byte-span encoding reads RGB and ignores the alpha byte.</remarks>
public static class BlurHashCodec
{
    /// <summary>The default number of horizontal cosine components when encoding.</summary>
    public const int DefaultComponentsX = 4;
    /// <summary>The default number of vertical cosine components when encoding.</summary>
    public const int DefaultComponentsY = 3;
    /// <summary>The default contrast multiplier when decoding.</summary>
    public const float DefaultPunch = 1f;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";
    private const int Base83Radix = 83;
    private const int MaximumComponents = 9;
    private const int DcDigits = 4;
    private const int AcDigits = 2;
    private const int AcStart = 2 + DcDigits;
    private const int EncodedLengthWithoutFirstFactor = AcStart - AcDigits;
    private const int MaximumDcValue = 0xffffff;
    private const int AcLevels = 19;
    private const int MaximumAcValue = AcLevels * AcLevels * AcLevels;
    private const int AcScale = Base83Radix * 2;
    private const int MaximumCachedCosines = 65_536;
    private const double SrgbDecodeThreshold = 0.04045;
    private const double SrgbEncodeThreshold = 0.0031308;
    private const double SrgbLinearScale = 12.92;
    private const double SrgbOffset = 0.055;
    private const double SrgbScale = 1.055;
    private const double SrgbGamma = 2.4;
    private static readonly double[] LinearRgb = CreateLinearRgbTable();

    /// <summary>Creates a BlurHash from a bitmap scaled to at most 100 pixels on its longest side.</summary>
    /// <param name="source">The source bitmap; ownership remains with the caller.</param>
    /// <param name="componentsX">Horizontal component count, from 1 to 9; defaults to 4.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9; defaults to 3.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(SKBitmap source, int componentsX = DefaultComponentsX, int componentsY = DefaultComponentsY)
    {
        ValidateComponents(componentsX, componentsY);
        using var normalized = PixelBuffers.Normalize(source, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, componentsX, componentsY);
    }

    /// <summary>Encodes a caller-owned image with 1 to 9 components on each axis.</summary>
    public static string Encode(SKImage source, int componentsX = DefaultComponentsX, int componentsY = DefaultComponentsY)
    {
        ValidateComponents(componentsX, componentsY);
        using var normalized = PixelBuffers.Normalize(source, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, componentsX, componentsY);
    }

    /// <summary>Encodes a caller-owned pixmap with 1 to 9 components on each axis.</summary>
    public static string Encode(SKPixmap source, int componentsX = DefaultComponentsX, int componentsY = DefaultComponentsY)
    {
        ValidateComponents(componentsX, componentsY);
        using var normalized = PixelBuffers.Normalize(source, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, componentsX, componentsY);
    }

    /// <summary>Encodes unpremultiplied row-major RGBA8 bytes as RGB BlurHash.</summary>
    /// <param name="rgba">Pixel bytes ending at the last pixel; earlier rows may have padding.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="stride">Bytes from one row start to the next.</param>
    /// <param name="componentsX">Horizontal component count, from 1 to 9; defaults to 4.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9; defaults to 3.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(ReadOnlySpan<byte> rgba, int width, int height, int stride,
        int componentsX = DefaultComponentsX, int componentsY = DefaultComponentsY)
    {
        PixelBuffers.Validate(rgba, width, height, stride, PixelBuffers.MaximumSourcePixels);
        ValidateComponents(componentsX, componentsY);

        Span<(double R, double G, double B)> factors = stackalloc (double, double, double)[componentsX * componentsY];
        var cosX = Cosines(width, componentsX);
        var cosY = Cosines(height, componentsY);
        try
        {
            // The DC term is the average linear color; each AC term uses twice the
            // cosine-basis average. Precompute the spatial basis for both axes.
            for (var cy = 0; cy < componentsY; cy++)
                for (var cx = 0; cx < componentsX; cx++)
                {
                    double r = 0, g = 0, b = 0;
                    for (var y = 0; y < height; y++)
                        for (var x = 0; x < width; x++)
                        {
                            var i = y * stride + x * PixelBuffers.RgbaBytesPerPixel;
                            var basis = cosX.Length != 0 && cosY.Length != 0
                                ? cosX[cx * width + x] * cosY[cy * height + y]
                                : Cosine(cosX, cx, x, width) * Cosine(cosY, cy, y, height);
                            r += basis * ToLinear(rgba[i]);
                            g += basis * ToLinear(rgba[i + 1]);
                            b += basis * ToLinear(rgba[i + 2]);
                        }
                    var scale = (cx == 0 && cy == 0 ? 1.0 : 2.0) / (width * height);
                    factors[cy * componentsX + cx] = (r * scale, g * scale, b * scale);
                }

            Span<char> result = stackalloc char[EncodedLengthWithoutFirstFactor + AcDigits * factors.Length];
            Write((componentsX - 1) + MaximumComponents * (componentsY - 1), result, 0, 1);
            double maximum = 0;
            for (var i = 1; i < factors.Length; i++)
                maximum = Math.Max(maximum, Math.Max(Math.Abs(factors[i].R), Math.Max(Math.Abs(factors[i].G), Math.Abs(factors[i].B))));
            // The base83 header stores component counts, the maximum AC magnitude,
            // 24-bit sRGB DC color, then two digits for each signed AC triplet.
            var quantized = factors.Length == 1 ? 0 : Math.Clamp((int)Math.Floor(maximum * AcScale - 0.5), 0, Base83Radix - 1);
            Write(quantized, result, 1, 1);
            var dc = factors[0];
            Write((ToSrgb(dc.R) << 16) | (ToSrgb(dc.G) << 8) | ToSrgb(dc.B), result, 2, DcDigits);
            var maxValue = (quantized + 1) / (double)AcScale;
            for (var i = 1; i < factors.Length; i++)
            {
                var ac = factors[i];
                Write(Quantize(ac.R, maxValue) * AcLevels * AcLevels +
                    Quantize(ac.G, maxValue) * AcLevels + Quantize(ac.B, maxValue), result,
                    AcStart + (i - 1) * AcDigits, AcDigits);
            }
            return new string(result);
        }
        finally
        {
            ReturnCosines(cosX);
            ReturnCosines(cosY);
        }
    }

    /// <summary>Encodes caller-owned RGBA8 array pixels; the array is never retained.</summary>
    public static string Encode(byte[] rgba, int width, int height, int stride,
        int componentsX = DefaultComponentsX, int componentsY = DefaultComponentsY)
    {
        ArgumentNullException.ThrowIfNull(rgba);
        return Encode(rgba.AsSpan(), width, height, stride, componentsX, componentsY);
    }

    /// <summary>Decodes a BlurHash to a caller-owned RGBA8 bitmap with adjustable color variation.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width, from 1 pixel within the 1 MP preview limit.</param>
    /// <param name="height">Exact output height, from 1 pixel within the 1 MP preview limit.</param>
    /// <param name="punch">Nonnegative, finite multiplier for AC color components; defaults to 1 (unchanged).</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(string hash, int width, int height, float punch = DefaultPunch)
    {
        PixelBuffers.ValidateDimensions(width, height, PixelBuffers.MaximumPreviewPixels);
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        try
        {
            using var pixmap = bitmap.PeekPixels() ?? throw new InvalidOperationException("Unable to allocate the decoded bitmap.");
            DecodeInto(hash, pixmap, punch);
            bitmap.NotifyPixelsChanged();
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    /// <summary>Decodes to a caller-owned image with the specified AC punch.</summary>
    public static SKImage DecodeImage(string hash, int width, int height, float punch = DefaultPunch)
    {
        using var bitmap = DecodeBitmap(hash, width, height, punch);
        return SKImage.FromBitmap(bitmap) ?? throw new InvalidOperationException("Unable to create the decoded image.");
    }

    /// <summary>Writes into caller-owned, unpremultiplied RGBA8888 pixmap pixels; the pixmap does not own its backing memory.</summary>
    public static void DecodeInto(string hash, SKPixmap destination, float punch = DefaultPunch)
    {
        ArgumentNullException.ThrowIfNull(hash);
        DecodeInto(hash.AsSpan(), destination, punch);
    }

    /// <summary>Writes into caller-owned, unpremultiplied RGBA8888 pixmap pixels.</summary>
    public static void DecodeInto(ReadOnlySpan<char> hash, SKPixmap destination, float punch = DefaultPunch)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var pixels = PixelBuffers.Destination(destination, destination.Width, destination.Height, PixelBuffers.MaximumPreviewPixels);
        DecodeInto(hash, pixels, destination.Width, destination.Height, destination.RowBytes, punch);
    }

    /// <summary>Decodes a BlurHash to tightly packed, opaque RGBA8 bytes with adjustable color variation.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width in pixels.</param>
    /// <param name="height">Exact output height in pixels.</param>
    /// <param name="punch">Nonnegative, finite multiplier for AC color components; defaults to 1 (unchanged).</param>
    /// <returns>Row-major RGBA8 bytes, with alpha equal to 255.</returns>
    public static byte[] Decode(string hash, int width, int height, float punch = DefaultPunch)
    {
        ArgumentNullException.ThrowIfNull(hash);
        return Decode(hash.AsSpan(), width, height, punch);
    }

    /// <summary>Decodes base83 characters into a caller-owned, tightly packed RGBA8 array.</summary>
    public static byte[] Decode(ReadOnlySpan<char> hash, int width, int height, float punch = DefaultPunch)
    {
        PixelBuffers.ValidateDimensions(width, height, PixelBuffers.MaximumSourcePixels);
        var pixels = new byte[checked(width * height * PixelBuffers.RgbaBytesPerPixel)];
        DecodeInto(hash, pixels, width, height, width * PixelBuffers.RgbaBytesPerPixel, punch);
        return pixels;
    }

    /// <summary>Writes decoded pixels into caller-owned RGBA8 memory. Earlier rows may have padding.</summary>
    public static void DecodeInto(ReadOnlySpan<char> hash, Span<byte> destination, int width, int height,
        int stride, float punch = DefaultPunch)
    {
        PixelBuffers.ValidateDestination(destination, width, height, stride, PixelBuffers.MaximumSourcePixels);
        if (!float.IsFinite(punch) || punch < 0)
            throw new ArgumentOutOfRangeException(nameof(punch), "Punch must be finite and nonnegative.");
        if (hash.Length < AcStart)
            throw new FormatException("BlurHash is too short.");
        var size = Read(hash, 0, 1);
        var nx = size % MaximumComponents + 1;
        var ny = size / MaximumComponents + 1;
        if (ny > MaximumComponents)
            throw new FormatException("Invalid BlurHash component header.");
        if (hash.Length != EncodedLengthWithoutFirstFactor + AcDigits * nx * ny)
            throw new FormatException("BlurHash has an incorrect number of components.");
        var maximum = (Read(hash, 1, 1) + 1) / (double)AcScale;
        var dc = Read(hash, 2, DcDigits);
        if (dc > MaximumDcValue)
            throw new FormatException("Invalid BlurHash DC value.");
        Span<(double R, double G, double B)> factors = stackalloc (double, double, double)[nx * ny];
        factors[0] = (ToLinear((byte)(dc >> 16)), ToLinear((byte)(dc >> 8)), ToLinear((byte)dc));
        for (var i = 1; i < factors.Length; i++)
        {
            var ac = Read(hash, AcStart + (i - 1) * AcDigits, AcDigits);
            if (ac >= MaximumAcValue)
                throw new FormatException("Invalid BlurHash AC value.");
            factors[i] = (Unquantize(ac / (AcLevels * AcLevels), maximum) * punch,
                Unquantize(ac / AcLevels % AcLevels, maximum) * punch, Unquantize(ac % AcLevels, maximum) * punch);
        }
        var cosX = Cosines(width, nx);
        var cosY = Cosines(height, ny);
        try
        {
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    double r = 0, g = 0, b = 0;
                    for (var cy = 0; cy < ny; cy++)
                        for (var cx = 0; cx < nx; cx++)
                        {
                            var factor = factors[cy * nx + cx];
                            var basis = cosX.Length != 0 && cosY.Length != 0
                                ? cosX[cx * width + x] * cosY[cy * height + y]
                                : Cosine(cosX, cx, x, width) * Cosine(cosY, cy, y, height);
                            r += basis * factor.R;
                            g += basis * factor.G;
                            b += basis * factor.B;
                        }
                    var index = y * stride + x * PixelBuffers.RgbaBytesPerPixel;
                    destination[index] = (byte)ToSrgb(r);
                    destination[index + 1] = (byte)ToSrgb(g);
                    destination[index + 2] = (byte)ToSrgb(b);
                    destination[index + 3] = byte.MaxValue;
                }
        }
        finally
        {
            ReturnCosines(cosX);
            ReturnCosines(cosY);
        }
    }

    private static void ValidateComponents(int componentsX, int componentsY)
    {
        if (componentsX is < 1 or > MaximumComponents || componentsY is < 1 or > MaximumComponents)
            throw new ArgumentOutOfRangeException(nameof(componentsX), "Both component counts must be from 1 to 9.");
    }

    private static double[] Cosines(int length, int count)
    {
        if ((long)length * count > MaximumCachedCosines)
            return Array.Empty<double>();
        var values = ArrayPool<double>.Shared.Rent(length * count);
        for (var c = 0; c < count; c++)
            for (var p = 0; p < length; p++)
                values[c * length + p] = Math.Cos(Math.PI * c * p / length);
        return values;
    }

    private static double Cosine(double[] cache, int component, int pixel, int length) =>
        cache.Length == 0 ? Math.Cos(Math.PI * component * pixel / length) : cache[component * length + pixel];

    private static void ReturnCosines(double[] cache)
    {
        if (cache.Length != 0)
            ArrayPool<double>.Shared.Return(cache);
    }

    private static int Quantize(double value, double max) =>
        Math.Clamp((int)Math.Floor(Math.CopySign(Math.Sqrt(Math.Abs(value / max)), value) * (AcLevels - 1) / 2 +
            AcLevels / 2.0), 0, AcLevels - 1);

    private static double Unquantize(int value, double max) =>
        Math.CopySign(Math.Pow(Math.Abs((value - (AcLevels - 1) / 2) / ((AcLevels - 1) / 2.0)), 2),
            value - (AcLevels - 1) / 2) * max;

    private static double[] CreateLinearRgbTable()
    {
        // Each pixel is visited once per component; table lookup avoids
        // repeating the sRGB transfer function for every frequency.
        var values = new double[byte.MaxValue + 1];
        for (var i = 0; i < values.Length; i++)
        {
            var value = i / (double)byte.MaxValue;
            values[i] = value <= SrgbDecodeThreshold
                ? value / SrgbLinearScale
                : Math.Pow((value + SrgbOffset) / SrgbScale, SrgbGamma);
        }
        return values;
    }

    private static double ToLinear(byte value) => LinearRgb[value];

    private static int ToSrgb(double value)
    {
        var v = Math.Clamp(value, 0, 1);
        return Math.Clamp((int)Math.Floor(byte.MaxValue * (v <= SrgbEncodeThreshold
            ? v * SrgbLinearScale : SrgbScale * Math.Pow(v, 1 / SrgbGamma) - SrgbOffset) + 0.5), 0, byte.MaxValue);
    }

    private static void Write(int value, Span<char> output, int offset, int count)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            output[offset + i] = Alphabet[value % Base83Radix];
            value /= Base83Radix;
        }
    }

    private static int Read(ReadOnlySpan<char> hash, int offset, int count)
    {
        var value = 0;
        for (var i = 0; i < count; i++)
        {
            var digit = Alphabet.IndexOf(hash[offset + i]);
            if (digit < 0)
                throw new FormatException("BlurHash contains a non-Base83 character.");
            value = value * Base83Radix + digit;
        }
        return value;
    }
}
