// Based on SkiaSharp.Extended's SKPixelComparer at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;

namespace SkiaSharp.Testing;

/// <summary>Compares decoded pixels without taking ownership of the supplied images, bitmaps or pixmaps.</summary>
internal static class SKPixelComparer
{
	public static SKPixelComparisonResult Compare(SKImage first, SKImage second) =>
		Compare(first, second, 0, null);

	public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKPixelComparerOptions? options) =>
		Compare(first, second, 0, options);

	public static SKPixelComparisonResult Compare(SKImage first, SKImage second, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateTolerance(tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return ComparePixels(a, b, null, tolerance, settings);
	}

	public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKImage mask, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateMask(first.Width, first.Height, mask);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var m = Normalize(mask, settings);
		return ComparePixels(a, b, m, 0, settings);
	}

	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second) =>
		Compare(first, second, 0, null);

	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options) =>
		Compare(first, second, 0, options);

	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateTolerance(tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return ComparePixels(a, b, null, tolerance, settings);
	}

	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKBitmap mask, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateMask(first.Width, first.Height, mask);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var m = Normalize(mask, settings);
		return ComparePixels(a, b, m, 0, settings);
	}

	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second) =>
		Compare(first, second, 0, null);

	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options) =>
		Compare(first, second, 0, options);

	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateTolerance(tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return ComparePixels(a, b, null, tolerance, settings);
	}

	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixmap mask, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateMask(first.Width, first.Height, mask);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var m = Normalize(mask, settings);
		return ComparePixels(a, b, m, 0, settings);
	}

	/// <summary>Creates a strict RGB black-and-white difference mask owned by the caller.</summary>
	public static SKImage GenerateDifferenceMask(SKImage first, SKImage second) =>
		GenerateDifferenceMask(first, second, null);

	public static SKImage GenerateDifferenceMask(SKImage first, SKImage second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.BinaryMask, 0, options);

	public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second) =>
		GenerateDifferenceMask(first, second, null);

	public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.BinaryMask, 0, options);

	public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second) =>
		GenerateDifferenceMask(first, second, null);

	public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.BinaryMask, 0, options);

	/// <summary>Raw RGB absolute differences; alpha-only differences are black even if alpha is compared.</summary>
	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second) =>
		GenerateDifferenceImage(first, second, null);

	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ChannelDelta, 0, options);

	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelDifferenceStyle style, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateStyleAndTolerance(style, tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return DifferenceImage(a, b, style, tolerance, settings);
	}

	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second) =>
		GenerateDifferenceImage(first, second, null);

	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ChannelDelta, 0, options);

	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelDifferenceStyle style, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateStyleAndTolerance(style, tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return DifferenceImage(a, b, style, tolerance, settings);
	}

	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second) =>
		GenerateDifferenceImage(first, second, null);

	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ChannelDelta, 0, options);

	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelDifferenceStyle style, int tolerance, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		ValidateStyleAndTolerance(style, tolerance);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		return DifferenceImage(a, b, style, tolerance, settings);
	}

	private static SKPixelComparisonResult ComparePixels(SKBitmap first, SKBitmap second, SKBitmap? mask, int tolerance, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		var errorPixels = 0;
		var maxDelta = 0;
		var absoluteError = 0L;
		var sumSquaredError = 0L;

		using var aPixmap = first.PeekPixels();
		using var bPixmap = second.PeekPixels();
		using var mPixmap = mask?.PeekPixels();
		if (aPixmap == null || bPixmap == null || (mask != null && mPixmap == null))
			throw new InvalidOperationException("Unable to access normalized pixels.");
		var a = aPixmap.GetPixelSpan<SKColor>();
		var b = bPixmap.GetPixelSpan<SKColor>();
		var m = mPixmap == null ? Span<SKColor>.Empty : mPixmap.GetPixelSpan<SKColor>();

		for (var i = 0; i < totalPixels; i++)
		{
			var delta = Evaluate(a[i], b[i], mask == null ? (SKColor?)null : m[i], tolerance, settings);
			absoluteError = checked(absoluteError + delta.AbsoluteError);
			sumSquaredError = checked(sumSquaredError + delta.SumSquaredError);
			if (delta.Rejected)
				errorPixels++;
			maxDelta = Math.Max(maxDelta, delta.MaxChannelDelta);
		}

		return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError, sumSquaredError, settings.ChannelCount, maxDelta);
	}

	private static SKImage DifferenceImage(SKBitmap first, SKBitmap second, SKPixelDifferenceStyle style, int tolerance, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		using var aPixmap = first.PeekPixels();
		using var bPixmap = second.PeekPixels();
		if (aPixmap == null || bPixmap == null)
			throw new InvalidOperationException("Unable to access normalized pixels.");
		var a = aPixmap.GetPixelSpan<SKColor>();
		var b = bPixmap.GetPixelSpan<SKColor>();

		using var bitmap = Allocate(new SKImageInfo(first.Width, first.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
		using var pixmap = bitmap.PeekPixels();
		if (pixmap == null)
			throw new InvalidOperationException("Unable to access difference pixels.");
		var pixels = pixmap.GetPixelSpan<SKColor>();
		for (var i = 0; i < totalPixels; i++)
		{
			var delta = Evaluate(a[i], b[i], null, tolerance, settings);
			pixels[i] = style switch
			{
				SKPixelDifferenceStyle.BinaryMask => delta.Rejected ? SKColors.White : SKColors.Black,
				SKPixelDifferenceStyle.ChannelDelta => new SKColor((byte)delta.Red, (byte)delta.Green, (byte)delta.Blue),
				_ => delta.Rejected ? SKColors.Red
					: delta.MaxChannelDelta > 0 ? new SKColor(255, 200, 0)
					: new SKColor((byte)(b[i].Red / 4), (byte)(b[i].Green / 4), (byte)(b[i].Blue / 4)),
			};
		}

		return SKImage.FromBitmap(bitmap) ?? throw new InvalidOperationException("Unable to create difference image.");
	}

	private static PixelDifference Evaluate(SKColor first, SKColor second, SKColor? mask, int tolerance, Settings settings)
	{
		var r = Math.Abs(second.Red - first.Red);
		var g = Math.Abs(second.Green - first.Green);
		var b = Math.Abs(second.Blue - first.Blue);
		var a = settings.CompareAlpha ? Math.Abs(second.Alpha - first.Alpha) : 0;
		var max = Math.Max(Math.Max(r, g), Math.Max(b, a));
		if (settings.TolerancePerChannel)
		{
			var er = r > (mask?.Red ?? tolerance) ? r : 0;
			var eg = g > (mask?.Green ?? tolerance) ? g : 0;
			var eb = b > (mask?.Blue ?? tolerance) ? b : 0;
			var ea = a > (mask?.Alpha ?? tolerance) ? a : 0;
			var error = er + eg + eb + ea;
			return new PixelDifference(r, g, b, max, error, (long)er * er + (long)eg * eg + (long)eb * eb + (long)ea * ea);
		}
		var sum = r + g + b + a;
		var threshold = mask.HasValue
			? mask.Value.Red + mask.Value.Green + mask.Value.Blue + (settings.CompareAlpha ? mask.Value.Alpha : 0)
			: tolerance;
		return sum > threshold
			? new PixelDifference(r, g, b, max, sum, (long)r * r + (long)g * g + (long)b * b + (long)a * a)
			: new PixelDifference(r, g, b, max, 0, 0);
	}

	private readonly struct PixelDifference
	{
		public PixelDifference(int red, int green, int blue, int maxChannelDelta, int absoluteError, long sumSquaredError)
		{
			Red = red;
			Green = green;
			Blue = blue;
			MaxChannelDelta = maxChannelDelta;
			AbsoluteError = absoluteError;
			SumSquaredError = sumSquaredError;
		}

		public int Red { get; }
		public int Green { get; }
		public int Blue { get; }
		public int MaxChannelDelta { get; }
		public int AbsoluteError { get; }
		public long SumSquaredError { get; }
		public bool Rejected => AbsoluteError > 0;
	}

	private readonly struct Settings
	{
		public Settings(SKPixelComparerOptions? options)
		{
			TolerancePerChannel = options?.TolerancePerChannel ?? true;
			CompareAlpha = options?.CompareAlpha ?? false;
			AlphaType = options?.AlphaType ?? SKAlphaType.Unpremul;
			if (AlphaType != SKAlphaType.Unpremul && AlphaType != SKAlphaType.Premul)
				throw new ArgumentOutOfRangeException(nameof(options), "AlphaType must be Unpremul or Premul.");
		}

		public bool TolerancePerChannel { get; }
		public bool CompareAlpha { get; }
		public SKAlphaType AlphaType { get; }
		public int ChannelCount => CompareAlpha ? 4 : 3;
	}

	private static SKBitmap Normalize(SKImage source, Settings settings)
	{
		var bitmap = Allocate(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, settings.AlphaType));
		try
		{
			using var destination = bitmap.PeekPixels();
			if (destination == null || !source.ReadPixels(destination))
				throw new InvalidOperationException("Unable to read image pixels.");
			// Validate readback, then retain Extended's draw normalization, including hidden RGB at alpha zero.
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.Transparent);
			canvas.DrawImage(source, 0, 0, SKSamplingOptions.Default);
			return bitmap;
		}
		catch
		{
			bitmap.Dispose();
			throw;
		}
	}

	private static SKBitmap Normalize(SKBitmap source, Settings settings)
	{
		using var pixmap = source.PeekPixels();
		if (pixmap == null)
			throw new InvalidOperationException("Unable to read bitmap pixels.");
		try
		{
			return Normalize(pixmap, settings);
		}
		finally
		{
			GC.KeepAlive(source);
		}
	}

	private static SKBitmap Normalize(SKPixmap source, Settings settings)
	{
		using var image = SKImage.FromPixels(source)
			?? throw new InvalidOperationException("Unable to wrap pixmap pixels.");
		try
		{
			return Normalize(image, settings);
		}
		finally
		{
			GC.KeepAlive(source);
		}
	}

	private static SKBitmap Allocate(SKImageInfo info)
	{
		_ = checked(info.Width * info.Height);
		_ = checked(info.Width * 4);
		_ = checked((long)info.Width * info.Height * 4);
		var bitmap = new SKBitmap();
		try
		{
			if (!bitmap.TryAllocPixels(info) || bitmap.RowBytes != checked(info.Width * 4) || bitmap.GetPixels() == IntPtr.Zero)
				throw new InvalidOperationException("Unable to allocate tightly packed BGRA pixels.");
			return bitmap;
		}
		catch
		{
			bitmap.Dispose();
			throw;
		}
	}

	private static void ValidatePair(SKImage first, SKImage second)
	{
		Validate(first, nameof(first));
		Validate(second, nameof(second));
		ValidateDimensions(first.Width, first.Height, second.Width, second.Height);
	}

	private static void ValidatePair(SKBitmap first, SKBitmap second)
	{
		Validate(first, nameof(first));
		Validate(second, nameof(second));
		ValidateDimensions(first.Width, first.Height, second.Width, second.Height);
	}

	private static void ValidatePair(SKPixmap first, SKPixmap second)
	{
		Validate(first, nameof(first));
		Validate(second, nameof(second));
		ValidateDimensions(first.Width, first.Height, second.Width, second.Height);
	}

	private static void ValidateMask(int width, int height, SKImage mask)
	{
		Validate(mask, nameof(mask));
		ValidateDimensions(width, height, mask.Width, mask.Height);
	}

	private static void ValidateMask(int width, int height, SKBitmap mask)
	{
		Validate(mask, nameof(mask));
		ValidateDimensions(width, height, mask.Width, mask.Height);
	}

	private static void ValidateMask(int width, int height, SKPixmap mask)
	{
		Validate(mask, nameof(mask));
		ValidateDimensions(width, height, mask.Width, mask.Height);
	}

	private static void Validate(SKImage image, string name)
	{
		if (image == null)
			throw new ArgumentNullException(name);
		if (image.Handle == IntPtr.Zero || image.Width <= 0 || image.Height <= 0)
			throw new ArgumentException("Image is disposed or empty.", name);
	}

	private static void Validate(SKBitmap bitmap, string name)
	{
		if (bitmap == null)
			throw new ArgumentNullException(name);
		if (bitmap.Handle == IntPtr.Zero || bitmap.Width <= 0 || bitmap.Height <= 0 || bitmap.GetPixels() == IntPtr.Zero)
			throw new ArgumentException("Bitmap is disposed, empty or unreadable.", name);
	}

	private static void Validate(SKPixmap pixmap, string name)
	{
		if (pixmap == null)
			throw new ArgumentNullException(name);
		if (pixmap.Handle == IntPtr.Zero || pixmap.Width <= 0 || pixmap.Height <= 0 || pixmap.GetPixels() == IntPtr.Zero)
			throw new ArgumentException("Pixmap is disposed, empty or unreadable.", name);
	}

	private static void ValidateDimensions(int width, int height, int otherWidth, int otherHeight)
	{
		if (width != otherWidth || height != otherHeight)
			throw new InvalidOperationException($"Unable to compare images of different sizes: {width}x{height} vs {otherWidth}x{otherHeight}.");
		_ = checked(width * height);
		_ = checked((long)width * height * 4);
	}

	private static void ValidateTolerance(int tolerance)
	{
		if (tolerance < 0)
			throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be non-negative.");
	}

	private static void ValidateStyleAndTolerance(SKPixelDifferenceStyle style, int tolerance)
	{
		ValidateTolerance(tolerance);
		if (style != SKPixelDifferenceStyle.BinaryMask &&
			style != SKPixelDifferenceStyle.ChannelDelta &&
			style != SKPixelDifferenceStyle.ThresholdOverlay)
			throw new ArgumentOutOfRangeException(nameof(style));
	}
}
