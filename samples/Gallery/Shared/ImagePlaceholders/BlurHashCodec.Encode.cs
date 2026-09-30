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
    /// <summary>Creates a BlurHash from a bitmap scaled to at most 100 pixels on its longest side.</summary>
    /// <param name="source">The source bitmap; ownership remains with the caller.</param>
    /// <param name="componentsX">Horizontal component count, from 1 to 9.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(SKBitmap source, int componentsX, int componentsY)
    {
        ValidateComponents(componentsX, componentsY);
        using var normalized = PixelBuffers.Normalize(source, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, componentsX, componentsY);
    }

    /// <summary>Encodes a caller-owned image with 1 to 9 components on each axis.</summary>
    public static string Encode(SKImage source, int componentsX, int componentsY)
    {
        ValidateComponents(componentsX, componentsY);
        ArgumentNullException.ThrowIfNull(source);
        PixelBuffers.ValidateDimensions(source.Width, source.Height, PixelBuffers.MaximumSourcePixels);
        using var pixels = PixelBuffers.PeekEncodingPixels(source, compositeWhite: true);
        if (pixels is not null)
            return Encode(PixelBuffers.Pixels(pixels), pixels.Width, pixels.Height, pixels.RowBytes, componentsX, componentsY);
        using var normalized = PixelBuffers.Normalize(source, PixelBuffers.MaximumThumbnailDimension, compositeWhite: true);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes, componentsX, componentsY);
    }

    /// <summary>Encodes a caller-owned pixmap with 1 to 9 components on each axis.</summary>
    public static string Encode(SKPixmap source, int componentsX, int componentsY)
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
    /// <param name="componentsX">Horizontal component count, from 1 to 9.</param>
    /// <param name="componentsY">Vertical component count, from 1 to 9.</param>
    /// <returns>The base83-encoded BlurHash.</returns>
    public static string Encode(ReadOnlySpan<byte> rgba, int width, int height, int stride,
        int componentsX, int componentsY)
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
            {
                for (var cx = 0; cx < componentsX; cx++)
                {
                    double r = 0, g = 0, b = 0;
                    for (var y = 0; y < height; y++)
                    {
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
                    }
                    var scale = (cx == 0 && cy == 0 ? 1.0 : 2.0) / (width * height);
                    factors[cy * componentsX + cx] = (r * scale, g * scale, b * scale);
                }
            }

            Span<char> result = stackalloc char[EncodedLengthWithoutFirstFactor + AcDigits * factors.Length];
            Write((componentsX - 1) + MaximumComponents * (componentsY - 1), result, 0, 1);
            double maximum = 0;
            for (var i = 1; i < factors.Length; i++)
            {
                maximum = Math.Max(maximum, Math.Max(Math.Abs(factors[i].R), Math.Max(Math.Abs(factors[i].G), Math.Abs(factors[i].B))));
            }
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

    private static void ValidateComponents(int componentsX, int componentsY)
    {
        if (componentsX is < 1 or > MaximumComponents || componentsY is < 1 or > MaximumComponents)
            throw new ArgumentOutOfRangeException(nameof(componentsX), "Both component counts must be from 1 to 9.");
    }

    private static int Quantize(double value, double max) =>
        Math.Clamp((int)Math.Floor(Math.CopySign(Math.Sqrt(Math.Abs(value / max)), value) * (AcLevels - 1) / 2 +
            AcLevels / 2.0), 0, AcLevels - 1);

    private static void Write(int value, Span<char> output, int offset, int count)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            output[offset + i] = Alphabet[value % Base83Radix];
            value /= Base83Radix;
        }
    }
}
