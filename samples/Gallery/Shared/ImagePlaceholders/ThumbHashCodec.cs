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

namespace SkiaSharpSample.ImagePlaceholders;

/// <summary>Encodes and decodes ThumbHash binary values, including transparency.</summary>
/// <remarks>Encoding requires unpremultiplied RGBA8 at no more than 100 by 100 pixels.
/// Base64 is a UI/storage choice, not part of the ThumbHash binary wire format.</remarks>
public static partial class ThumbHashCodec
{
    private const int MaximumInputDimension = PixelBuffers.MaximumThumbnailDimension;
    private const int OpaqueHeaderBytes = 5;
    private const int AlphaHeaderBytes = 6;
    private const int BitsPerByte = 8;
    private const int BitsPerNibble = 4;
    private const int NibblesPerByte = 2;
    private const int PScaleShift = 3;
    private const int QScaleShift = 9;
    private const int PColorShift = 6;
    private const int QColorShift = 12;
    private const int LScaleShift = 18;
    private const int ShortAxisMask = 7;
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
    private const double ChromaColorCenter = 31.5;
    private const double AcDecodeCenter = 7.5;
    private const int MaximumCachedCosines = 65_536;

    private static int Count(int nx, int ny)
    {
        var count = 0;
        for (var cy = 0; cy < ny; cy++)
        {
            for (var cx = 0; cx * ny < nx * (ny - cy); cx++)
            {
                count++;
            }
        }
        return count;
    }

    private static int Round(double x) => (int)Math.Floor(x + 0.5);
}
