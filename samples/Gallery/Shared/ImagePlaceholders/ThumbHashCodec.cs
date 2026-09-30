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

/// <summary>Encodes and decodes ThumbHash binary values, including transparency.</summary>
/// <remarks>Encoding requires unpremultiplied RGBA8 at no more than 100 by 100 pixels.
/// Base64 is a UI/storage choice, not part of the ThumbHash binary wire format.</remarks>
public static class ThumbHashCodec
{
    private const int MaximumInputDimension = 100;
    private const int OpaqueLuminanceComponents = 7;
    private const int AlphaLuminanceComponents = 5;
    private const int ChromaComponents = 3;
    private const int AlphaComponents = 5;
    private const int MaximumSixBitValue = 63;
    private const int MaximumFiveBitValue = 31;
    private const int MaximumNibbleValue = 15;
    private const int HasAlphaBit = 23;
    private const int LandscapeBit = 15;
    private const double ChromaDecodeScale = 1.25;
    private const int MaximumCachedCosines = 65_536;

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
        PixelBuffers.Validate(rgba, width, height, stride, 10_000);

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

    /// <summary>Encodes caller-owned RGBA8 array pixels; the array is never retained.</summary>
    public static byte[] Encode(byte[] rgba, int width, int height, int stride)
    {
        ArgumentNullException.ThrowIfNull(rgba);
        return Encode(rgba.AsSpan(), width, height, stride);
    }

    private static byte[] EncodeCore(ReadOnlySpan<byte> rgba, int width, int height, int stride,
        double[] l, double[] p, double[] q, double[] a, double[] basisX, double[] basisY)
    {
        var count = width * height;
        double averageR = 0, averageG = 0, averageB = 0, averageA = 0;
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * stride + 4 * x;
                var alpha = rgba[i + 3] / 255.0;
                averageR += alpha * rgba[i] / 255.0;
                averageG += alpha * rgba[i + 1] / 255.0;
                averageB += alpha * rgba[i + 2] / 255.0;
                averageA += alpha;
            }
        if (averageA > 0)
        {
            averageR /= averageA;
            averageG /= averageA;
            averageB /= averageA;
        }
        // Blend transparent RGB toward the image's visible mean before the LPQ
        // transform; encode alpha separately when any pixel is not fully opaque.
        var hasAlpha = averageA < count;
        var limit = hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents;
        var lx = Math.Max(1, Round(limit * width / (double)Math.Max(width, height)));
        var ly = Math.Max(1, Round(limit * height / (double)Math.Max(width, height)));
        for (var cx = 0; cx < OpaqueLuminanceComponents; cx++)
            for (var x = 0; x < width; x++)
                basisX[cx * width + x] = Math.Cos(Math.PI / width * cx * (x + 0.5));
        for (var cy = 0; cy < OpaqueLuminanceComponents; cy++)
            for (var y = 0; y < height; y++)
                basisY[cy * height + y] = Math.Cos(Math.PI / height * cy * (y + 0.5));
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * stride + 4 * x;
                var index = y * width + x;
                var alpha = rgba[i + 3] / 255.0;
                var r = averageR * (1 - alpha) + alpha * rgba[i] / 255.0;
                var g = averageG * (1 - alpha) + alpha * rgba[i + 1] / 255.0;
                var b = averageB * (1 - alpha) + alpha * rgba[i + 2] / 255.0;
                l[index] = (r + g + b) / 3;
                p[index] = (r + g) / 2 - b;
                q[index] = r - g;
                a[index] = alpha;
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
            (Round(31.5 + 31.5 * yellowBlue.Dc) << 6) |
            (Round(31.5 + 31.5 * redGreen.Dc) << 12) |
            (Round(MaximumFiveBitValue * luminance.Scale) << 18) |
            (hasAlpha ? 1 << HasAlphaBit : 0);
        var header16 = (landscape ? ly : lx) |
            (Round(MaximumSixBitValue * yellowBlue.Scale) << 3) |
            (Round(MaximumSixBitValue * redGreen.Scale) << 9) |
            (landscape ? 1 << LandscapeBit : 0);
        var nibbles = lAc.Length + pAc.Length + qAc.Length + aAc.Length;
        var start = hasAlpha ? 6 : 5;
        var result = new byte[start + (nibbles + 1) / 2];
        result[0] = (byte)header24;
        result[1] = (byte)(header24 >> 8);
        result[2] = (byte)(header24 >> 16);
        result[3] = (byte)header16;
        result[4] = (byte)(header16 >> 8);
        if (hasAlpha)
            result[5] = (byte)(Round(MaximumNibbleValue * alphaChannel.Dc) | Round(MaximumNibbleValue * alphaChannel.Scale) << 4);
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
            result[start + nibble / 2] |= (byte)(Round(MaximumNibbleValue * coefficient) << (4 * (nibble++ % 2)));
    }

    /// <summary>Decodes binary ThumbHash to a caller-owned RGBA8 bitmap within 64 by 64 pixels.</summary>
    /// <param name="hash">Binary ThumbHash bytes (decode Base64 first if stored as text).</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(ReadOnlySpan<byte> hash) => DecodeBitmap(hash, 64, 64);

    /// <summary>Decodes binary ThumbHash to a caller-owned RGBA8 bitmap, preserving its encoded aspect ratio.</summary>
    /// <param name="hash">Binary ThumbHash bytes (decode Base64 first if stored as text).</param>
    /// <param name="maxWidth">Maximum output width, within the 1 MP preview limit.</param>
    /// <param name="maxHeight">Maximum output height, within the 1 MP preview limit.</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, 1_000_000);
        var (width, height) = GetDecodedSize(hash, maxWidth, maxHeight);
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        try
        {
            using var pixmap = bitmap.PeekPixels() ?? throw new InvalidOperationException("Unable to allocate the decoded bitmap.");
            DecodeInto(hash, pixmap, maxWidth, maxHeight);
            bitmap.NotifyPixelsChanged();
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    /// <summary>Decodes binary ThumbHash into a caller-owned image; dispose it after use.</summary>
    public static SKImage DecodeImage(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight)
    {
        using var bitmap = DecodeBitmap(hash, maxWidth, maxHeight);
        return SKImage.FromBitmap(bitmap) ?? throw new InvalidOperationException("Unable to create the decoded image.");
    }

    /// <summary>Decodes into a caller-owned RGBA8888 unpremultiplied pixmap of the encoded aspect ratio.</summary>
    /// <remarks>The caller retains ownership of the pixmap and its backing pixels.</remarks>
    public static void DecodeInto(ReadOnlySpan<byte> hash, SKPixmap destination, int maxWidth, int maxHeight)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, 1_000_000);
        var (width, height) = GetDecodedSize(hash, maxWidth, maxHeight);
        var pixels = PixelBuffers.Destination(destination, width, height, 1_000_000);
        DecodeInto(hash, pixels, maxWidth, maxHeight, destination.RowBytes, out _, out _);
    }

    /// <summary>Decodes binary ThumbHash to tightly packed, unpremultiplied RGBA8 bytes.</summary>
    /// <param name="hash">Binary ThumbHash bytes.</param>
    /// <param name="maxWidth">Maximum output width in pixels.</param>
    /// <param name="maxHeight">Maximum output height in pixels.</param>
    /// <param name="width">Actual decoded width in pixels.</param>
    /// <param name="height">Actual decoded height in pixels.</param>
    /// <returns>Row-major RGBA8 bytes; the encoded aspect ratio selects the output dimensions.</returns>
    public static byte[] Decode(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight, out int width, out int height)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, 16_000_000);
        var (decodedWidth, decodedHeight) = GetDecodedSize(hash, maxWidth, maxHeight);
        var result = new byte[checked(decodedWidth * decodedHeight * 4)];
        DecodeInto(hash, result, maxWidth, maxHeight, decodedWidth * 4, out _, out _);
        width = decodedWidth;
        height = decodedHeight;
        return result;
    }

    /// <summary>Decodes caller-owned binary ThumbHash bytes into a new RGBA8 array.</summary>
    public static byte[] Decode(byte[] hash, int maxWidth, int maxHeight, out int width, out int height)
    {
        ArgumentNullException.ThrowIfNull(hash);
        return Decode(hash.AsSpan(), maxWidth, maxHeight, out width, out height);
    }

    /// <summary>Writes decoded unpremultiplied RGBA8 into caller-owned memory, preserving any row padding.</summary>
    /// <param name="hash">Binary ThumbHash bytes.</param>
    /// <param name="destination">Destination buffer with at least the actual decoded dimensions.</param>
    /// <param name="maxWidth">Maximum decoded width.</param>
    /// <param name="maxHeight">Maximum decoded height.</param>
    /// <param name="stride">Destination row stride in bytes.</param>
    /// <param name="width">Actual decoded width.</param>
    /// <param name="height">Actual decoded height.</param>
    public static void DecodeInto(ReadOnlySpan<byte> hash, Span<byte> destination, int maxWidth, int maxHeight,
        int stride, out int width, out int height)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, 16_000_000);
        var (decodedWidth, decodedHeight) = GetDecodedSize(hash, maxWidth, maxHeight);
        PixelBuffers.ValidateDestination(destination, decodedWidth, decodedHeight, stride, 16_000_000);
        var header24 = hash[0] | hash[1] << 8 | hash[2] << 16;
        var header16 = hash[3] | hash[4] << 8;
        var hasAlpha = (header24 & (1 << HasAlphaBit)) != 0;
        var landscape = (header16 & (1 << LandscapeBit)) != 0;
        var shortAxis = header16 & 7;
        var lx = Math.Max(ChromaComponents, landscape ? hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents : shortAxis);
        var ly = Math.Max(ChromaComponents, landscape ? shortAxis : hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        var start = hasAlpha ? 6 : 5;
        var lDc = (header24 & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var pDc = ((header24 >> 6) & 63) / 31.5 - 1;
        var qDc = ((header24 >> 12) & 63) / 31.5 - 1;
        var aDc = hasAlpha ? (hash[5] & MaximumNibbleValue) / (double)MaximumNibbleValue : 1;
        var lScale = ((header24 >> 18) & MaximumFiveBitValue) / (double)MaximumFiveBitValue;
        var pScale = ((header16 >> 3) & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var qScale = ((header16 >> 9) & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var aScale = hasAlpha ? (hash[5] >> 4) / (double)MaximumNibbleValue : 0;
        var nibble = 0;
        var l = ReadChannel(hash, start, lx, ly, lScale, ref nibble);
        var p = ReadChannel(hash, start, ChromaComponents, ChromaComponents, pScale * ChromaDecodeScale, ref nibble);
        var q = ReadChannel(hash, start, ChromaComponents, ChromaComponents, qScale * ChromaDecodeScale, ref nibble);
        var a = hasAlpha ? ReadChannel(hash, start, AlphaComponents, AlphaComponents, aScale, ref nibble) : Array.Empty<double>();

        var countX = Math.Max(lx, hasAlpha ? AlphaComponents : ChromaComponents);
        var countY = Math.Max(ly, hasAlpha ? AlphaComponents : ChromaComponents);
        var cosX = (long)decodedWidth * countX <= MaximumCachedCosines
            ? ArrayPool<double>.Shared.Rent(decodedWidth * countX) : Array.Empty<double>();
        var cosY = (long)decodedHeight * countY <= MaximumCachedCosines
            ? ArrayPool<double>.Shared.Rent(decodedHeight * countY) : Array.Empty<double>();
        try
        {
            for (var cx = 0; cx < countX && cosX.Length != 0; cx++)
                for (var x = 0; x < decodedWidth; x++)
                    cosX[cx * decodedWidth + x] = Math.Cos(Math.PI * cx * (x + 0.5) / decodedWidth);
            for (var cy = 0; cy < countY && cosY.Length != 0; cy++)
                for (var y = 0; y < decodedHeight; y++)
                    cosY[cy * decodedHeight + y] = Math.Cos(Math.PI * cy * (y + 0.5) / decodedHeight);
            for (var y = 0; y < decodedHeight; y++)
                for (var x = 0; x < decodedWidth; x++)
                {
                    var luminance = lDc + Sum(l, lx, ly, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var yellowBlue = pDc + Sum(p, ChromaComponents, ChromaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var redGreen = qDc + Sum(q, ChromaComponents, ChromaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var alpha = aDc + (hasAlpha ? Sum(a, AlphaComponents, AlphaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY) : 0);
                    var blue = luminance - 2.0 / 3 * yellowBlue;
                    var red = (3 * luminance - blue + redGreen) / 2;
                    var green = red - redGreen;
                    var i = y * stride + 4 * x;
                    destination[i] = ToByte(red);
                    destination[i + 1] = ToByte(green);
                    destination[i + 2] = ToByte(blue);
                    destination[i + 3] = ToByte(alpha);
                }
        }
        finally
        {
            if (cosX.Length != 0)
                ArrayPool<double>.Shared.Return(cosX);
            if (cosY.Length != 0)
                ArrayPool<double>.Shared.Return(cosY);
        }
        width = decodedWidth;
        height = decodedHeight;
    }

    /// <summary>Reads the encoded aspect ratio and returns decoded dimensions within the supplied bounds, without allocating a pixel buffer.</summary>
    public static (int Width, int Height) GetDecodedSize(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, 16_000_000);
        if (hash.Length < 5)
            throw new FormatException("ThumbHash is too short.");
        var header24 = hash[0] | hash[1] << 8 | hash[2] << 16;
        var header16 = hash[3] | hash[4] << 8;
        var hasAlpha = (header24 & (1 << HasAlphaBit)) != 0;
        var landscape = (header16 & (1 << LandscapeBit)) != 0;
        var shortAxis = header16 & 7;
        if (shortAxis < 1 || shortAxis > (hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents))
            throw new FormatException("Invalid ThumbHash aspect header.");
        var lx = Math.Max(ChromaComponents, landscape ? hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents : shortAxis);
        var ly = Math.Max(ChromaComponents, landscape ? shortAxis : hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        var nibbleCount = Count(lx, ly) - 1 + 2 * (Count(ChromaComponents, ChromaComponents) - 1) +
            (hasAlpha ? Count(AlphaComponents, AlphaComponents) - 1 : 0);
        var start = hasAlpha ? 6 : 5;
        if (hash.Length != start + (nibbleCount + 1) / 2 || (nibbleCount % 2 == 1 && (hash[^1] & 0xf0) != 0))
            throw new FormatException("Invalid ThumbHash byte length or padding.");
        var ratio = landscape
            ? (hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents) / (double)shortAxis
            : shortAxis / (double)(hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        return (Math.Max(1, Math.Min(maxWidth, Round(maxHeight * ratio))),
            Math.Max(1, Math.Min(maxHeight, Round(maxWidth / ratio))));
    }

    private static (double Dc, double Scale) EncodeChannel(double[] channel, int width, int height,
        int nx, int ny, double[] basisX, double[] basisY, Span<double> ac)
    {
        // LPQA samples a triangular subset of a 2D cosine transform. DC is
        // the mean, while ACs are normalized by their largest magnitude.
        double dc = 0, scale = 0;
        var index = 0;
        for (var cy = 0; cy < ny; cy++)
            for (var cx = 0; cx * ny < nx * (ny - cy); cx++)
            {
                double factor = 0;
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        factor += channel[y * width + x] *
                            basisX[cx * width + x] * basisY[cy * height + y];
                factor /= width * height;
                if (cx == 0 && cy == 0)
                    dc = factor;
                else
                {
                    ac[index++] = factor;
                    scale = Math.Max(scale, Math.Abs(factor));
                }
            }
        if (scale > 0)
            for (var i = 0; i < ac.Length; i++)
                ac[i] = 0.5 + 0.5 * ac[i] / scale;
        return (dc, scale);
    }

    private static int Count(int nx, int ny)
    {
        var count = 0;
        for (var cy = 0; cy < ny; cy++)
            for (var cx = 0; cx * ny < nx * (ny - cy); cx++)
                count++;
        return count;
    }

    private static double[] ReadChannel(ReadOnlySpan<byte> bytes, int start, int nx, int ny, double scale, ref int nibble)
    {
        var ac = new double[Count(nx, ny) - 1];
        for (var i = 0; i < ac.Length; i++)
        {
            ac[i] = (((bytes[start + nibble / 2] >> (4 * (nibble % 2))) & 15) / 7.5 - 1) * scale;
            nibble++;
        }
        return ac;
    }

    private static double Sum(double[] ac, int nx, int ny, int x, int y, int width, int height, double[] cosX, double[] cosY)
    {
        double sum = 0;
        var index = 0;
        for (var cy = 0; cy < ny; cy++)
            for (var cx = cy == 0 ? 1 : 0; cx * ny < nx * (ny - cy); cx++)
                sum += 2 * ac[index++] *
                    (cosX.Length == 0 ? Math.Cos(Math.PI * cx * (x + 0.5) / width) : cosX[cx * width + x]) *
                    (cosY.Length == 0 ? Math.Cos(Math.PI * cy * (y + 0.5) / height) : cosY[cy * height + y]);
        return sum;
    }

    private static int Round(double x) => (int)Math.Floor(x + 0.5);
    private static byte ToByte(double x) => (byte)Math.Clamp((int)(Math.Clamp(x, 0, 1) * 255), 0, 255);
}
