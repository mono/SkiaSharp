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
using SkiaSharp;

namespace SkiaSharpSample.ImagePlaceholders;

/// <summary>Encodes and decodes standard RGB BlurHash base83 strings.</summary>
/// <remarks>BlurHash has no alpha channel. Bitmap encoding composites transparent pixels over white;
/// byte-span encoding reads RGB and ignores the alpha byte.</remarks>
public static class BlurHashCodec
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";
    private const int Base83Radix = 83;
    private const int MaximumComponents = 9;
    private const int AcLevels = 19;
    private const int MaximumAcValue = AcLevels * AcLevels * AcLevels;
    private const int AcScale = Base83Radix * 2;
    private static readonly double[] LinearRgb = CreateLinearRgbTable();

    /// <summary>Creates a BlurHash with 4 horizontal and 3 vertical components.</summary>
    /// <param name="source">The source bitmap; ownership remains with the caller.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(SKBitmap source) => Encode(source, 4, 3);

    /// <summary>Creates a BlurHash from a bitmap scaled to at most 100 pixels on its longest side.</summary>
    /// <param name="source">The source bitmap; ownership remains with the caller.</param>
    /// <param name="componentsX">Horizontal component count, from 1 to 9.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(SKBitmap source, int componentsX, int componentsY)
    {
        ValidateComponents(componentsX, componentsY);
        var (rgba, width, height) = PixelBuffers.FromBitmap(source, 100, compositeWhite: true);
        return Encode(rgba, width, height, width * 4, componentsX, componentsY);
    }

    /// <summary>Encodes unpremultiplied row-major RGBA8 bytes as RGB BlurHash.</summary>
    /// <param name="rgba">Pixel bytes ending at the last pixel; earlier rows may have padding.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="stride">Bytes from one row start to the next.</param>
    /// <param name="componentsX">Horizontal component count, from 1 to 9.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(ReadOnlySpan<byte> rgba, int width, int height, int stride, int componentsX, int componentsY)
    {
        PixelBuffers.Validate(rgba, width, height, stride, 16_000_000);
        ValidateComponents(componentsX, componentsY);

        var factors = new (double R, double G, double B)[componentsX * componentsY];
        var cosX = Cosines(width, componentsX);
        var cosY = Cosines(height, componentsY);
        // The DC term is the average linear color; each AC term uses twice the
        // cosine-basis average. Precompute the spatial basis for both axes.
        for (var cy = 0; cy < componentsY; cy++)
        for (var cx = 0; cx < componentsX; cx++)
        {
            double r = 0, g = 0, b = 0;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * stride + x * 4;
                var basis = cosX[cx * width + x] * cosY[cy * height + y];
                r += basis * ToLinear(rgba[i]);
                g += basis * ToLinear(rgba[i + 1]);
                b += basis * ToLinear(rgba[i + 2]);
            }
            var scale = (cx == 0 && cy == 0 ? 1.0 : 2.0) / (width * height);
            factors[cy * componentsX + cx] = (r * scale, g * scale, b * scale);
        }

        var result = new char[4 + 2 * factors.Length];
        Write((componentsX - 1) + MaximumComponents * (componentsY - 1), result, 0, 1);
        double maximum = 0;
        for (var i = 1; i < factors.Length; i++)
            maximum = Math.Max(maximum, Math.Max(Math.Abs(factors[i].R), Math.Max(Math.Abs(factors[i].G), Math.Abs(factors[i].B))));
        // The base83 header stores component counts, the maximum AC magnitude,
        // 24-bit sRGB DC color, then two digits for each signed AC triplet.
        var quantized = factors.Length == 1 ? 0 : Math.Clamp((int)Math.Floor(maximum * AcScale - 0.5), 0, Base83Radix - 1);
        Write(quantized, result, 1, 1);
        var dc = factors[0];
        Write((ToSrgb(dc.R) << 16) | (ToSrgb(dc.G) << 8) | ToSrgb(dc.B), result, 2, 4);
        var maxValue = (quantized + 1) / (double)AcScale;
        for (var i = 1; i < factors.Length; i++)
        {
            var ac = factors[i];
            Write(Quantize(ac.R, maxValue) * AcLevels * AcLevels +
                Quantize(ac.G, maxValue) * AcLevels + Quantize(ac.B, maxValue), result, 4 + i * 2, 2);
        }
        return new string(result);
    }

    /// <summary>Decodes a BlurHash to a caller-owned 64 by 64 RGBA8 bitmap.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(string hash) => DecodeBitmap(hash, 64, 64);

    /// <summary>Decodes a BlurHash to a caller-owned RGBA8 bitmap.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width, from 1 pixel within the 1 MP preview limit.</param>
    /// <param name="height">Exact output height, from 1 pixel within the 1 MP preview limit.</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(string hash, int width, int height) => DecodeBitmap(hash, width, height, 1f);

    /// <summary>Decodes a BlurHash to a caller-owned RGBA8 bitmap with adjustable color variation.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width, from 1 pixel within the 1 MP preview limit.</param>
    /// <param name="height">Exact output height, from 1 pixel within the 1 MP preview limit.</param>
    /// <param name="punch">Nonnegative, finite multiplier for AC color components; 1 preserves the original preview, 0 shows its average color.</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(string hash, int width, int height, float punch)
    {
        PixelBuffers.ValidateDimensions(width, height, 1_000_000);
        return PixelBuffers.ToBitmap(Decode(hash, width, height, punch), width, height);
    }

    /// <summary>Decodes a BlurHash to tightly packed, opaque RGBA8 bytes.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width in pixels.</param>
    /// <param name="height">Exact output height in pixels.</param>
    /// <returns>Row-major RGBA8 bytes, with alpha equal to 255.</returns>
    public static byte[] Decode(string hash, int width, int height) => Decode(hash, width, height, 1f);

    /// <summary>Decodes a BlurHash to tightly packed, opaque RGBA8 bytes with adjustable color variation.</summary>
    /// <param name="hash">A standard base83 BlurHash.</param>
    /// <param name="width">Exact output width in pixels.</param>
    /// <param name="height">Exact output height in pixels.</param>
    /// <param name="punch">Nonnegative, finite multiplier for AC color components; 1 preserves the original preview, 0 shows its average color.</param>
    /// <returns>Row-major RGBA8 bytes, with alpha equal to 255.</returns>
    public static byte[] Decode(string hash, int width, int height, float punch)
    {
        PixelBuffers.ValidateDimensions(width, height, 16_000_000);
        if (!float.IsFinite(punch) || punch < 0)
            throw new ArgumentOutOfRangeException(nameof(punch), "Punch must be finite and nonnegative.");
        ArgumentNullException.ThrowIfNull(hash);
        if (hash.Length < 6)
            throw new FormatException("BlurHash is too short.");
        var size = Read(hash, 0, 1);
        var nx = size % MaximumComponents + 1;
        var ny = size / MaximumComponents + 1;
        if (ny > MaximumComponents)
            throw new FormatException("Invalid BlurHash component header.");
        if (hash.Length != 4 + 2 * nx * ny)
            throw new FormatException("BlurHash has an incorrect number of components.");
        var maximum = (Read(hash, 1, 1) + 1) / (double)AcScale;
        var dc = Read(hash, 2, 4);
        if (dc > 0xffffff)
            throw new FormatException("Invalid BlurHash DC value.");
        var factors = new (double R, double G, double B)[nx * ny];
        factors[0] = (ToLinear((byte)(dc >> 16)), ToLinear((byte)(dc >> 8)), ToLinear((byte)dc));
        for (var i = 1; i < factors.Length; i++)
        {
            var ac = Read(hash, 4 + 2 * i, 2);
            if (ac >= MaximumAcValue)
                throw new FormatException("Invalid BlurHash AC value.");
            factors[i] = (Unquantize(ac / (AcLevels * AcLevels), maximum) * punch,
                Unquantize(ac / AcLevels % AcLevels, maximum) * punch, Unquantize(ac % AcLevels, maximum) * punch);
        }
        var pixels = new byte[checked(width * height * 4)];
        var cosX = Cosines(width, nx);
        var cosY = Cosines(height, ny);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            double r = 0, g = 0, b = 0;
            for (var cy = 0; cy < ny; cy++)
            for (var cx = 0; cx < nx; cx++)
            {
                var factor = factors[cy * nx + cx];
                var basis = cosX[cx * width + x] * cosY[cy * height + y];
                r += basis * factor.R;
                g += basis * factor.G;
                b += basis * factor.B;
            }
            var index = (y * width + x) * 4;
            pixels[index] = (byte)ToSrgb(r);
            pixels[index + 1] = (byte)ToSrgb(g);
            pixels[index + 2] = (byte)ToSrgb(b);
            pixels[index + 3] = 255;
        }
        return pixels;
    }

    private static void ValidateComponents(int componentsX, int componentsY)
    {
        if (componentsX is < 1 or > MaximumComponents || componentsY is < 1 or > MaximumComponents)
            throw new ArgumentOutOfRangeException(nameof(componentsX), "Both component counts must be from 1 to 9.");
    }

    private static double[] Cosines(int length, int count)
    {
        var values = new double[length * count];
        for (var c = 0; c < count; c++)
        for (var p = 0; p < length; p++)
            values[c * length + p] = Math.Cos(Math.PI * c * p / length);
        return values;
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
        var values = new double[256];
        for (var i = 0; i < values.Length; i++)
        {
            var value = i / 255.0;
            values[i] = value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        return values;
    }

    private static double ToLinear(byte value) => LinearRgb[value];

    private static int ToSrgb(double value)
    {
        var v = Math.Clamp(value, 0, 1);
        return Math.Clamp((int)Math.Floor(255 * (v <= 0.0031308 ? v * 12.92 : 1.055 * Math.Pow(v, 1 / 2.4) - 0.055) + 0.5), 0, 255);
    }

    private static void Write(int value, char[] output, int offset, int count)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            output[offset + i] = Alphabet[value % Base83Radix];
            value /= Base83Radix;
        }
    }

    private static int Read(string hash, int offset, int count)
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
