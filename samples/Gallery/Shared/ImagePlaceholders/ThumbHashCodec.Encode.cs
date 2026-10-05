// ThumbHash reference implementation by Evan Wallace:
// https://github.com/evanw/thumbhash
// Copyright (c) 2023 Evan Wallace
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

public static partial class ThumbHashCodec
{
    /// <summary>Creates a binary ThumbHash from a bitmap scaled to at most 100 pixels on its longest side.</summary>
    /// <param name="source">The source bitmap; ownership remains with the caller.</param>
    /// <returns>The binary ThumbHash bytes.</returns>
    public static byte[] Encode(SKBitmap source)
    {
        using var normalized = PixelBuffers.Normalize(source, MaximumInputDimension, compositeWhite: false);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes);
    }

    /// <summary>Encodes a caller-owned image, normalized to at most 100 by 100 pixels.</summary>
    public static byte[] Encode(SKImage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        PixelBuffers.ValidateDimensions(source.Width, source.Height, PixelBuffers.MaximumSourcePixels);
        using var pixels = PixelBuffers.PeekEncodingPixels(source, compositeWhite: false);
        if (pixels is not null)
            return Encode(PixelBuffers.Pixels(pixels), pixels.Width, pixels.Height, pixels.RowBytes);
        using var normalized = PixelBuffers.Normalize(source, MaximumInputDimension, compositeWhite: false);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes);
    }

    /// <summary>Encodes a caller-owned pixmap, normalized to at most 100 by 100 pixels.</summary>
    public static byte[] Encode(SKPixmap source)
    {
        using var normalized = PixelBuffers.Normalize(source, MaximumInputDimension, compositeWhite: false);
        return Encode(PixelBuffers.Pixels(normalized), normalized.Width, normalized.Height, normalized.RowBytes);
    }

    /// <summary>Encodes unpremultiplied row-major RGBA8 bytes as a binary ThumbHash.</summary>
    /// <param name="rgba">Pixel bytes ending at the last pixel; earlier rows may have padding.</param>
    /// <param name="width">Image width, from 1 to 100 pixels.</param>
    /// <param name="height">Image height, from 1 to 100 pixels.</param>
    /// <param name="stride">Bytes from one row start to the next.</param>
    /// <returns>The binary ThumbHash bytes, which can be stored as Base64 if text is needed.</returns>
    public static byte[] Encode(ReadOnlySpan<byte> rgba, int width, int height, int stride)
    {
        if (width > MaximumInputDimension || height > MaximumInputDimension)
            throw new ArgumentOutOfRangeException(nameof(width), "ThumbHash inputs must fit in 100 by 100 pixels.");
        PixelBuffers.Validate(rgba, width, height, stride, MaximumInputDimension * MaximumInputDimension);

        var count = width * height;
        var pool = ArrayPool<double>.Shared;
        var l = pool.Rent(count);
        var p = pool.Rent(count);
        var q = pool.Rent(count);
        var a = pool.Rent(count);
        var basisX = pool.Rent(width * OpaqueLuminanceComponents);
        var basisY = pool.Rent(height * OpaqueLuminanceComponents);
        try
        {
            return EncodeCore(rgba, width, height, stride, l, p, q, a, basisX, basisY);
        }
        finally
        {
            pool.Return(l);
            pool.Return(p);
            pool.Return(q);
            pool.Return(a);
            pool.Return(basisX);
            pool.Return(basisY);
        }
    }

    private static byte[] EncodeCore(ReadOnlySpan<byte> rgba, int width, int height, int stride,
        double[] l, double[] p, double[] q, double[] a, double[] basisX, double[] basisY)
    {
        // Weight the mean by alpha so fully transparent RGB cannot tint the
        // visible colors used to fill gaps in the low-frequency preview.
        var count = width * height;
        double averageR = 0, averageG = 0, averageB = 0, averageA = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = y * stride + PixelBuffers.RgbaBytesPerPixel * x;
                var alpha = rgba[i + 3] / (double)byte.MaxValue;
                averageR += alpha * rgba[i] / (double)byte.MaxValue;
                averageG += alpha * rgba[i + 1] / (double)byte.MaxValue;
                averageB += alpha * rgba[i + 2] / (double)byte.MaxValue;
                averageA += alpha;
            }
        }
        if (averageA > 0)
        {
            averageR /= averageA;
            averageG /= averageA;
            averageB /= averageA;
        }

        // Alpha-bearing hashes reserve coefficients for opacity, leaving fewer
        // luminance terms; size the two axes in proportion to the source.
        var hasAlpha = averageA < count;
        var limit = hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents;
        var lx = Math.Max(1, Round(limit * width / (double)Math.Max(width, height)));
        var ly = Math.Max(1, Round(limit * height / (double)Math.Max(width, height)));

        // Cache the spatial cosine basis once for all color and alpha channels.
        for (var cx = 0; cx < OpaqueLuminanceComponents; cx++)
        {
            for (var x = 0; x < width; x++)
            {
                basisX[cx * width + x] = Math.Cos(Math.PI / width * cx * (x + 0.5));
            }
        }
        for (var cy = 0; cy < OpaqueLuminanceComponents; cy++)
        {
            for (var y = 0; y < height; y++)
            {
                basisY[cy * height + y] = Math.Cos(Math.PI / height * cy * (y + 0.5));
            }
        }

        // Blend transparent RGB toward the visible mean, then decompose it
        // into luminance (L), yellow-blue (P), and red-green (Q). Keep opacity
        // (A) as its own transform channel.
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = y * stride + PixelBuffers.RgbaBytesPerPixel * x;
                var index = y * width + x;
                var alpha = rgba[i + 3] / (double)byte.MaxValue;
                var r = averageR * (1 - alpha) + alpha * rgba[i] / (double)byte.MaxValue;
                var g = averageG * (1 - alpha) + alpha * rgba[i + 1] / (double)byte.MaxValue;
                var b = averageB * (1 - alpha) + alpha * rgba[i + 2] / (double)byte.MaxValue;
                l[index] = (r + g + b) / 3;
                p[index] = (r + g) / 2 - b;
                q[index] = r - g;
                a[index] = alpha;
            }
        }

        // At most 27 luminance ACs and 14 alpha ACs fit on the stack; large
        // per-pixel workspaces remain pooled by Encode.
        Span<double> lAc = stackalloc double[Count(Math.Max(ChromaComponents, lx), Math.Max(ChromaComponents, ly)) - 1];
        Span<double> pAc = stackalloc double[Count(ChromaComponents, ChromaComponents) - 1];
        Span<double> qAc = stackalloc double[Count(ChromaComponents, ChromaComponents) - 1];
        Span<double> aAc = hasAlpha ? stackalloc double[Count(AlphaComponents, AlphaComponents) - 1] : Span<double>.Empty;

        var luminance = EncodeChannel(l, width, height, Math.Max(ChromaComponents, lx), Math.Max(ChromaComponents, ly), basisX, basisY, lAc);
        var yellowBlue = EncodeChannel(p, width, height, ChromaComponents, ChromaComponents, basisX, basisY, pAc);
        var redGreen = EncodeChannel(q, width, height, ChromaComponents, ChromaComponents, basisX, basisY, qAc);
        var alphaChannel = hasAlpha
            ? EncodeChannel(a, width, height, AlphaComponents, AlphaComponents, basisX, basisY, aAc)
            : default;

        var landscape = width > height;
        // ThumbHash packs little-endian header fields as:
        // [L DC:6, P DC:6, Q DC:6, L scale:5, alpha flag:1],
        // [short axis:3, P scale:6, Q scale:6, landscape flag:1].
        // A transparent hash adds [alpha DC:4, alpha scale:4]; ACs follow as nibbles.
        var header24 = Round(MaximumSixBitValue * luminance.Dc) |
            (Round(ChromaColorCenter + ChromaColorCenter * yellowBlue.Dc) << PColorShift) |
            (Round(ChromaColorCenter + ChromaColorCenter * redGreen.Dc) << QColorShift) |
            (Round(MaximumFiveBitValue * luminance.Scale) << LScaleShift) |
            (hasAlpha ? 1 << HasAlphaBit : 0);
        var header16 = (landscape ? ly : lx) |
            (Round(MaximumSixBitValue * yellowBlue.Scale) << PScaleShift) |
            (Round(MaximumSixBitValue * redGreen.Scale) << QScaleShift) |
            (landscape ? 1 << LandscapeBit : 0);

        var nibbles = lAc.Length + pAc.Length + qAc.Length + aAc.Length;
        var start = hasAlpha ? AlphaHeaderBytes : OpaqueHeaderBytes;
        var result = new byte[start + (nibbles + 1) / NibblesPerByte];
        result[0] = (byte)header24;
        result[1] = (byte)(header24 >> BitsPerByte);
        result[2] = (byte)(header24 >> (2 * BitsPerByte));
        result[3] = (byte)header16;
        result[4] = (byte)(header16 >> BitsPerByte);
        if (hasAlpha)
            result[5] = (byte)(Round(MaximumNibbleValue * alphaChannel.Dc) |
                Round(MaximumNibbleValue * alphaChannel.Scale) << BitsPerNibble);

        // Append four-bit AC coefficients in L, P, Q, A order after the header.
        var nibble = 0;
        WriteAc(lAc, result, start, ref nibble);
        WriteAc(pAc, result, start, ref nibble);
        WriteAc(qAc, result, start, ref nibble);
        WriteAc(aAc, result, start, ref nibble);
        return result;
    }

    private static void WriteAc(ReadOnlySpan<double> channel, byte[] result, int start, ref int nibble)
    {
        foreach (var coefficient in channel)
            result[start + nibble / NibblesPerByte] |= (byte)(Round(MaximumNibbleValue * coefficient) <<
                (BitsPerNibble * (nibble++ % NibblesPerByte)));
    }

    private static (double Dc, double Scale) EncodeChannel(double[] channel, int width, int height,
        int nx, int ny, double[] basisX, double[] basisY, Span<double> ac)
    {
        // LPQA samples a triangular subset of a 2D cosine transform. DC is
        // the mean, while ACs are normalized by their largest magnitude.
        double dc = 0, scale = 0;
        var index = 0;
        for (var cy = 0; cy < ny; cy++)
        {
            for (var cx = 0; cx * ny < nx * (ny - cy); cx++)
            {
                double factor = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        factor += channel[y * width + x] *
                            basisX[cx * width + x] * basisY[cy * height + y];
                    }
                }
                factor /= width * height;
                if (cx == 0 && cy == 0)
                    dc = factor;
                else
                {
                    ac[index++] = factor;
                    scale = Math.Max(scale, Math.Abs(factor));
                }
            }
        }

        if (scale > 0)
        {
            for (var i = 0; i < ac.Length; i++)
            {
                ac[i] = 0.5 + 0.5 * ac[i] / scale;
            }
        }
        return (dc, scale);
    }
}
