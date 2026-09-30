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

namespace SkiaSharpSample.ImagePlaceholders;

/// <summary>Encodes and decodes standard RGB BlurHash base83 strings.</summary>
/// <remarks>BlurHash has no alpha channel. Bitmap encoding composites transparent pixels over white;
/// byte-span encoding reads RGB and ignores the alpha byte.</remarks>
public static partial class BlurHashCodec
{
    private const float DefaultPunch = 1f;

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

    private static double[] Cosines(int length, int count)
    {
        if ((long)length * count > MaximumCachedCosines)
            return Array.Empty<double>();
        var values = ArrayPool<double>.Shared.Rent(length * count);
        for (var c = 0; c < count; c++)
        {
            for (var p = 0; p < length; p++)
            {
                values[c * length + p] = Math.Cos(Math.PI * c * p / length);
            }
        }
        return values;
    }

    private static double Cosine(double[] cache, int component, int pixel, int length) =>
        cache.Length == 0 ? Math.Cos(Math.PI * component * pixel / length) : cache[component * length + pixel];

    private static void ReturnCosines(double[] cache)
    {
        if (cache.Length != 0)
            ArrayPool<double>.Shared.Return(cache);
    }

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
}
