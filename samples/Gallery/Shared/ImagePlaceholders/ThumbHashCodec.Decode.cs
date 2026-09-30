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
    /// <summary>Decodes binary ThumbHash to a caller-owned RGBA8 bitmap, preserving its encoded aspect ratio.</summary>
    /// <param name="hash">Binary ThumbHash bytes (decode Base64 first if stored as text).</param>
    /// <param name="maxWidth">Maximum output width, within the 1 MP preview limit.</param>
    /// <param name="maxHeight">Maximum output height, within the 1 MP preview limit.</param>
    /// <returns>A bitmap to dispose after use.</returns>
    public static SKBitmap DecodeBitmap(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, PixelBuffers.MaximumPreviewPixels);
        var size = GetDecodedSize(hash, maxWidth, maxHeight);
        var bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
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
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, PixelBuffers.MaximumPreviewPixels);
        var size = GetDecodedSize(hash, maxWidth, maxHeight);
        var pixels = PixelBuffers.Destination(destination, size.Width, size.Height, PixelBuffers.MaximumPreviewPixels);
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
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, PixelBuffers.MaximumSourcePixels);
        var size = GetDecodedSize(hash, maxWidth, maxHeight);
        var result = new byte[checked(size.Width * size.Height * PixelBuffers.RgbaBytesPerPixel)];
        DecodeInto(hash, result, maxWidth, maxHeight, size.Width * PixelBuffers.RgbaBytesPerPixel, out _, out _);
        width = size.Width;
        height = size.Height;
        return result;
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
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, PixelBuffers.MaximumSourcePixels);
        var size = GetDecodedSize(hash, maxWidth, maxHeight);
        var decodedWidth = size.Width;
        var decodedHeight = size.Height;
        PixelBuffers.ValidateDestination(destination, decodedWidth, decodedHeight, stride, PixelBuffers.MaximumSourcePixels);
        var header24 = hash[0] | hash[1] << BitsPerByte | hash[2] << (2 * BitsPerByte);
        var header16 = hash[3] | hash[4] << BitsPerByte;
        var hasAlpha = (header24 & (1 << HasAlphaBit)) != 0;
        var landscape = (header16 & (1 << LandscapeBit)) != 0;
        var shortAxis = header16 & ShortAxisMask;
        var lx = Math.Max(ChromaComponents, landscape ? hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents : shortAxis);
        var ly = Math.Max(ChromaComponents, landscape ? shortAxis : hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        var start = hasAlpha ? AlphaHeaderBytes : OpaqueHeaderBytes;
        var lDc = (header24 & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var pDc = ((header24 >> PColorShift) & MaximumSixBitValue) / ChromaColorCenter - 1;
        var qDc = ((header24 >> QColorShift) & MaximumSixBitValue) / ChromaColorCenter - 1;
        var aDc = hasAlpha ? (hash[5] & MaximumNibbleValue) / (double)MaximumNibbleValue : 1;
        var lScale = ((header24 >> LScaleShift) & MaximumFiveBitValue) / (double)MaximumFiveBitValue;
        var pScale = ((header16 >> PScaleShift) & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var qScale = ((header16 >> QScaleShift) & MaximumSixBitValue) / (double)MaximumSixBitValue;
        var aScale = hasAlpha ? (hash[5] >> BitsPerNibble) / (double)MaximumNibbleValue : 0;
        var nibble = 0;
        // GetDecodedSize validated the header, bounding these stack buffers to 27 luminance and 14 alpha ACs.
        Span<double> l = stackalloc double[Count(lx, ly) - 1];
        Span<double> p = stackalloc double[Count(ChromaComponents, ChromaComponents) - 1];
        Span<double> q = stackalloc double[Count(ChromaComponents, ChromaComponents) - 1];
        Span<double> a = hasAlpha ? stackalloc double[Count(AlphaComponents, AlphaComponents) - 1] : Span<double>.Empty;
        ReadChannel(hash, start, l, lScale, ref nibble);
        ReadChannel(hash, start, p, pScale * ChromaDecodeScale, ref nibble);
        ReadChannel(hash, start, q, qScale * ChromaDecodeScale, ref nibble);
        if (hasAlpha)
            ReadChannel(hash, start, a, aScale, ref nibble);

        var countX = Math.Max(lx, hasAlpha ? AlphaComponents : ChromaComponents);
        var countY = Math.Max(ly, hasAlpha ? AlphaComponents : ChromaComponents);
        var cosX = (long)decodedWidth * countX <= MaximumCachedCosines
            ? ArrayPool<double>.Shared.Rent(decodedWidth * countX) : Array.Empty<double>();
        var cosY = (long)decodedHeight * countY <= MaximumCachedCosines
            ? ArrayPool<double>.Shared.Rent(decodedHeight * countY) : Array.Empty<double>();
        try
        {
            for (var cx = 0; cx < countX && cosX.Length != 0; cx++)
            {
                for (var x = 0; x < decodedWidth; x++)
                {
                    cosX[cx * decodedWidth + x] = Math.Cos(Math.PI * cx * (x + 0.5) / decodedWidth);
                }
            }
            for (var cy = 0; cy < countY && cosY.Length != 0; cy++)
            {
                for (var y = 0; y < decodedHeight; y++)
                {
                    cosY[cy * decodedHeight + y] = Math.Cos(Math.PI * cy * (y + 0.5) / decodedHeight);
                }
            }
            for (var y = 0; y < decodedHeight; y++)
            {
                for (var x = 0; x < decodedWidth; x++)
                {
                    var luminance = lDc + Sum(l, lx, ly, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var yellowBlue = pDc + Sum(p, ChromaComponents, ChromaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var redGreen = qDc + Sum(q, ChromaComponents, ChromaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY);
                    var alpha = aDc + (hasAlpha ? Sum(a, AlphaComponents, AlphaComponents, x, y, decodedWidth, decodedHeight, cosX, cosY) : 0);
                    var blue = luminance - 2.0 / 3 * yellowBlue;
                    var red = (3 * luminance - blue + redGreen) / 2;
                    var green = red - redGreen;
                    var i = y * stride + PixelBuffers.RgbaBytesPerPixel * x;
                    destination[i] = ToByte(red);
                    destination[i + 1] = ToByte(green);
                    destination[i + 2] = ToByte(blue);
                    destination[i + 3] = ToByte(alpha);
                }
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
    public static SKSizeI GetDecodedSize(ReadOnlySpan<byte> hash, int maxWidth, int maxHeight)
    {
        PixelBuffers.ValidateDimensions(maxWidth, maxHeight, PixelBuffers.MaximumSourcePixels);
        if (hash.Length < OpaqueHeaderBytes)
            throw new FormatException("ThumbHash is too short.");
        var header24 = hash[0] | hash[1] << BitsPerByte | hash[2] << (2 * BitsPerByte);
        var header16 = hash[3] | hash[4] << BitsPerByte;
        var hasAlpha = (header24 & (1 << HasAlphaBit)) != 0;
        var landscape = (header16 & (1 << LandscapeBit)) != 0;
        var shortAxis = header16 & ShortAxisMask;
        if (shortAxis < 1 || shortAxis > (hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents))
            throw new FormatException("Invalid ThumbHash aspect header.");
        var lx = Math.Max(ChromaComponents, landscape ? hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents : shortAxis);
        var ly = Math.Max(ChromaComponents, landscape ? shortAxis : hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        var nibbleCount = Count(lx, ly) - 1 + 2 * (Count(ChromaComponents, ChromaComponents) - 1) +
            (hasAlpha ? Count(AlphaComponents, AlphaComponents) - 1 : 0);
        var start = hasAlpha ? AlphaHeaderBytes : OpaqueHeaderBytes;
        if (hash.Length != start + (nibbleCount + 1) / NibblesPerByte ||
            (nibbleCount % NibblesPerByte == 1 && (hash[^1] & (MaximumNibbleValue << BitsPerNibble)) != 0))
            throw new FormatException("Invalid ThumbHash byte length or padding.");
        var ratio = landscape
            ? (hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents) / (double)shortAxis
            : shortAxis / (double)(hasAlpha ? AlphaLuminanceComponents : OpaqueLuminanceComponents);
        return new SKSizeI(Math.Max(1, Math.Min(maxWidth, Round(maxHeight * ratio))),
            Math.Max(1, Math.Min(maxHeight, Round(maxWidth / ratio))));
    }

    private static void ReadChannel(ReadOnlySpan<byte> bytes, int start, Span<double> ac, double scale, ref int nibble)
    {
        for (var i = 0; i < ac.Length; i++)
        {
            ac[i] = (((bytes[start + nibble / NibblesPerByte] >>
                (BitsPerNibble * (nibble % NibblesPerByte))) & MaximumNibbleValue) / AcDecodeCenter - 1) * scale;
            nibble++;
        }
    }

    private static double Sum(ReadOnlySpan<double> ac, int nx, int ny, int x, int y, int width, int height, double[] cosX, double[] cosY)
    {
        double sum = 0;
        var index = 0;
        for (var cy = 0; cy < ny; cy++)
        {
            for (var cx = cy == 0 ? 1 : 0; cx * ny < nx * (ny - cy); cx++)
            {
                sum += 2 * ac[index++] *
                    (cosX.Length == 0 ? Math.Cos(Math.PI * cx * (x + 0.5) / width) : cosX[cx * width + x]) *
                    (cosY.Length == 0 ? Math.Cos(Math.PI * cy * (y + 0.5) / height) : cosY[cy * height + y]);
            }
        }
        return sum;
    }

    private static byte ToByte(double x) => (byte)Math.Clamp((int)(Math.Clamp(x, 0, 1) * byte.MaxValue), 0, byte.MaxValue);
}
