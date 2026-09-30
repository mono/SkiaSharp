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

public static partial class BlurHashCodec
{
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
            {
                for (var x = 0; x < width; x++)
                {
                    double r = 0, g = 0, b = 0;
                    for (var cy = 0; cy < ny; cy++)
                    {
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
                    }
                    var index = y * stride + x * PixelBuffers.RgbaBytesPerPixel;
                    destination[index] = (byte)ToSrgb(r);
                    destination[index + 1] = (byte)ToSrgb(g);
                    destination[index + 2] = (byte)ToSrgb(b);
                    destination[index + 3] = byte.MaxValue;
                }
            }
        }
        finally
        {
            ReturnCosines(cosX);
            ReturnCosines(cosY);
        }
    }

    private static double Unquantize(int value, double max) =>
        Math.CopySign(Math.Pow(Math.Abs((value - (AcLevels - 1) / 2) / ((AcLevels - 1) / 2.0)), 2),
            value - (AcLevels - 1) / 2) * max;

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
