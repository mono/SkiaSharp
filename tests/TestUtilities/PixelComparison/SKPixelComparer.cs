// Based on SkiaSharp.Extended's SKPixelComparer at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;

namespace SkiaSharp.Testing;

/// <summary>Compares two decoded images, bitmaps, or pixmaps after normalization to tightly packed BGRA8888.</summary>
/// <remarks>Inputs and optional tolerance masks are borrowed for a synchronous call; do not mutate or dispose them concurrently. Results retain no native inputs. Difference images are independent, caller-owned, and must be disposed.</remarks>
public static class SKPixelComparer
{
	/// <summary>Compares two images with exact RGBA and unpremultiplied normalization.</summary>
	public static SKPixelComparisonResult Compare(SKImage first, SKImage second) =>
		Compare(first, second, null);

	/// <summary>Compares two images with snapshotted per-channel tolerances, optional mask, and whole-image match budgets.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, settings);
	}

	/// <summary>Compares two bitmaps with exact RGBA and unpremultiplied normalization.</summary>
	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second) =>
		Compare(first, second, null);

	/// <summary>Compares two bitmaps with snapshotted per-channel tolerances, optional mask, and whole-image match budgets.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, settings);
	}

	/// <summary>Compares two pixmaps with exact RGBA and unpremultiplied normalization.</summary>
	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second) =>
		Compare(first, second, null);

	/// <summary>Compares two pixmaps with snapshotted per-channel tolerances, optional mask, and whole-image match budgets.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, settings);
	}

	/// <summary>Creates a caller-owned opaque black/white difference image using exact RGBA.</summary>
	public static SKImage GenerateDifferenceMask(SKImage first, SKImage second) =>
		GenerateDifferenceMask(first, second, null);

	/// <summary>Creates a caller-owned black/white image using the same rejection predicate as <see cref="Compare(SKImage, SKImage, SKPixelComparerOptions)"/>; match budgets do not affect pixels.</summary>
	public static SKImage GenerateDifferenceMask(SKImage first, SKImage second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.BinaryMask);

	/// <summary>Creates a caller-owned opaque black/white difference image using exact RGBA.</summary>
	public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second) =>
		GenerateDifferenceMask(first, second, null);

	/// <summary>Creates a caller-owned black/white image using the same rejection predicate as <see cref="Compare(SKBitmap, SKBitmap, SKPixelComparerOptions)"/>; match budgets do not affect pixels.</summary>
	public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.BinaryMask);

	/// <summary>Creates a caller-owned opaque black/white difference image using exact RGBA.</summary>
	public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second) =>
		GenerateDifferenceMask(first, second, null);

	/// <summary>Creates a caller-owned black/white image using the same rejection predicate as <see cref="Compare(SKPixmap, SKPixmap, SKPixelComparerOptions)"/>; match budgets do not affect pixels.</summary>
	public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.BinaryMask);

	/// <summary>Creates a caller-owned threshold overlay using exact RGBA; alpha-only failures are visible.</summary>
	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second) =>
		GenerateDifferenceImage(first, second, null);

	/// <summary>Creates a caller-owned threshold overlay; match budgets do not affect its colors.</summary>
	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.ThresholdOverlay);

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and an optional borrowed mask.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, style, settings);
	}

	/// <summary>Creates a caller-owned threshold overlay using exact RGBA; alpha-only failures are visible.</summary>
	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second) =>
		GenerateDifferenceImage(first, second, null);

	/// <summary>Creates a caller-owned threshold overlay; match budgets do not affect its colors.</summary>
	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.ThresholdOverlay);

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and an optional borrowed mask.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, style, settings);
	}

	/// <summary>Creates a caller-owned threshold overlay using exact RGBA; alpha-only failures are visible.</summary>
	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second) =>
		GenerateDifferenceImage(first, second, null);

	/// <summary>Creates a caller-owned threshold overlay; match budgets do not affect its colors.</summary>
	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options) =>
		GenerateDifferenceImage(first, second, options, SKPixelDifferenceStyle.ThresholdOverlay);

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and an optional borrowed mask.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed.</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, style, settings);
	}

	private static SKBitmap? NormalizeMask(int width, int height, Settings settings)
	{
		if (settings.Mask == null)
			return null;
		ValidateMask(width, height, settings.Mask);
		return Normalize(settings.Mask, settings);
	}

	private static SKPixelComparisonResult ComparePixels(SKBitmap first, SKBitmap second, SKBitmap? mask, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		var errorPixels = 0;
		var maxDelta = 0;
		var rawAbsolute = 0L;
		var rawSquared = 0L;
		var filteredAbsolute = 0L;
		var filteredSquared = 0L;
		var left = first.Width;
		var top = first.Height;
		var right = 0;
		var bottom = 0;

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
			var delta = Evaluate(a[i], b[i], mask == null ? (SKColor?)null : m[i], settings.Tolerance);
			rawAbsolute = checked(rawAbsolute + delta.RawAbsolute);
			rawSquared = checked(rawSquared + delta.RawSquared);
			filteredAbsolute = checked(filteredAbsolute + delta.FilteredAbsolute);
			filteredSquared = checked(filteredSquared + delta.FilteredSquared);
			if (delta.Rejected)
			{
				errorPixels++;
				var x = i % first.Width;
				var y = i / first.Width;
				left = Math.Min(left, x);
				top = Math.Min(top, y);
				right = Math.Max(right, x + 1);
				bottom = Math.Max(bottom, y + 1);
			}
			maxDelta = Math.Max(maxDelta, delta.MaxChannelDelta);
		}

		var raw = new SKPixelComparisonMetrics(totalPixels, settings.Tolerance.ChannelCount, rawAbsolute, rawSquared);
		var filtered = new SKPixelComparisonMetrics(totalPixels, settings.Tolerance.ChannelCount, filteredAbsolute, filteredSquared);
		var budgets = settings.MaxErrorPixels.HasValue || settings.MaxErrorPixelFraction.HasValue ||
			settings.MaxNormalizedRootMeanSquaredError.HasValue;
		var isMatch = budgets
			? (!settings.MaxErrorPixels.HasValue || errorPixels <= settings.MaxErrorPixels.Value) &&
				(!settings.MaxErrorPixelFraction.HasValue || errorPixels <= Math.Floor(totalPixels * settings.MaxErrorPixelFraction.Value)) &&
				(!settings.MaxNormalizedRootMeanSquaredError.HasValue ||
					raw.NormalizedRootMeanSquaredError <= settings.MaxNormalizedRootMeanSquaredError.Value)
			: errorPixels == 0;
		return new SKPixelComparisonResult(totalPixels, settings.Tolerance.ChannelCount, errorPixels, maxDelta,
			errorPixels == 0 ? (SKRectI?)null : new SKRectI(left, top, right, bottom), raw, filtered, isMatch);
	}

	private static SKImage DifferenceImage(SKBitmap first, SKBitmap second, SKBitmap? mask, SKPixelDifferenceStyle style, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		using var aPixmap = first.PeekPixels();
		using var bPixmap = second.PeekPixels();
		using var mPixmap = mask?.PeekPixels();
		if (aPixmap == null || bPixmap == null || (mask != null && mPixmap == null))
			throw new InvalidOperationException("Unable to access normalized pixels.");
		var a = aPixmap.GetPixelSpan<SKColor>();
		var b = bPixmap.GetPixelSpan<SKColor>();
		var m = mPixmap == null ? Span<SKColor>.Empty : mPixmap.GetPixelSpan<SKColor>();

		using var bitmap = Allocate(new SKImageInfo(first.Width, first.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
		using var pixmap = bitmap.PeekPixels();
		if (pixmap == null)
			throw new InvalidOperationException("Unable to access difference pixels.");
		var pixels = pixmap.GetPixelSpan<SKColor>();
		for (var i = 0; i < totalPixels; i++)
		{
			var delta = Evaluate(a[i], b[i], mask == null ? (SKColor?)null : m[i], settings.Tolerance);
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

	private static PixelDifference Evaluate(SKColor first, SKColor second, SKColor? mask, SKPixelTolerance tolerance)
	{
		var r = Math.Abs(second.Red - first.Red);
		var g = Math.Abs(second.Green - first.Green);
		var b = Math.Abs(second.Blue - first.Blue);
		var a = Math.Abs(second.Alpha - first.Alpha);
		var rawAbsolute = 0;
		var rawSquared = 0L;
		var filteredAbsolute = 0;
		var filteredSquared = 0L;
		var max = 0;
		Accumulate(r, tolerance.Red, mask?.Red, ref rawAbsolute, ref rawSquared, ref filteredAbsolute, ref filteredSquared, ref max);
		Accumulate(g, tolerance.Green, mask?.Green, ref rawAbsolute, ref rawSquared, ref filteredAbsolute, ref filteredSquared, ref max);
		Accumulate(b, tolerance.Blue, mask?.Blue, ref rawAbsolute, ref rawSquared, ref filteredAbsolute, ref filteredSquared, ref max);
		Accumulate(a, tolerance.Alpha, mask?.Alpha, ref rawAbsolute, ref rawSquared, ref filteredAbsolute, ref filteredSquared, ref max);
		return new PixelDifference(r, g, b, max, rawAbsolute, rawSquared, filteredAbsolute, filteredSquared);
	}

	private static void Accumulate(int delta, double? allowance, byte? maskAllowance, ref int rawAbsolute, ref long rawSquared,
		ref int filteredAbsolute, ref long filteredSquared, ref int max)
	{
		if (!allowance.HasValue)
			return;
		rawAbsolute += delta;
		rawSquared += (long)delta * delta;
		max = Math.Max(max, delta);
		if (delta > Math.Max(allowance.Value, maskAllowance ?? 0))
		{
			filteredAbsolute += delta;
			filteredSquared += (long)delta * delta;
		}
	}

	private readonly struct PixelDifference
	{
		public PixelDifference(int red, int green, int blue, int maxChannelDelta, int rawAbsolute, long rawSquared,
			int filteredAbsolute, long filteredSquared)
		{
			Red = red;
			Green = green;
			Blue = blue;
			MaxChannelDelta = maxChannelDelta;
			RawAbsolute = rawAbsolute;
			RawSquared = rawSquared;
			FilteredAbsolute = filteredAbsolute;
			FilteredSquared = filteredSquared;
		}

		public int Red { get; }
		public int Green { get; }
		public int Blue { get; }
		public int MaxChannelDelta { get; }
		public int RawAbsolute { get; }
		public long RawSquared { get; }
		public int FilteredAbsolute { get; }
		public long FilteredSquared { get; }
		public bool Rejected => FilteredAbsolute > 0;
	}

	private readonly struct Settings
	{
		public Settings(SKPixelComparerOptions? options)
		{
			Tolerance = options?.Tolerance ?? SKPixelTolerance.Exact;
			AlphaType = options?.AlphaType ?? SKAlphaType.Unpremul;
			Mask = options?.ToleranceMask;
			MaxErrorPixels = options?.MaxErrorPixels;
			MaxErrorPixelFraction = options?.MaxErrorPixelFraction;
			MaxNormalizedRootMeanSquaredError = options?.MaxNormalizedRootMeanSquaredError;
			if (AlphaType != SKAlphaType.Unpremul && AlphaType != SKAlphaType.Premul)
				throw new ArgumentOutOfRangeException(nameof(options), "AlphaType must be Unpremul or Premul.");
			if (MaxErrorPixels < 0)
				throw new ArgumentOutOfRangeException(nameof(options), "MaxErrorPixels must be nonnegative.");
			if (InvalidFraction(MaxErrorPixelFraction))
				throw new ArgumentOutOfRangeException(nameof(options), "MaxErrorPixelFraction must be finite and between 0 and 1.");
			if (InvalidFraction(MaxNormalizedRootMeanSquaredError))
				throw new ArgumentOutOfRangeException(nameof(options), "MaxNormalizedRootMeanSquaredError must be finite and between 0 and 1.");
		}

		public SKPixelTolerance Tolerance { get; }
		public SKAlphaType AlphaType { get; }
		public SKImage? Mask { get; }
		public long? MaxErrorPixels { get; }
		public double? MaxErrorPixelFraction { get; }
		public double? MaxNormalizedRootMeanSquaredError { get; }

		private static bool InvalidFraction(double? value) =>
			value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value < 0 || value > 1);
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
		if (bitmap.Handle == IntPtr.Zero || bitmap.Width <= 0 || bitmap.Height <= 0 || bitmap.GetPixels() == IntPtr.Zero ||
			bitmap.RowBytes < bitmap.Info.RowBytes)
			throw new ArgumentException("Bitmap is disposed, empty or unreadable.", name);
	}

	private static void Validate(SKPixmap pixmap, string name)
	{
		if (pixmap == null)
			throw new ArgumentNullException(name);
		if (pixmap.Handle == IntPtr.Zero || pixmap.Width <= 0 || pixmap.Height <= 0 || pixmap.GetPixels() == IntPtr.Zero ||
			pixmap.RowBytes < pixmap.Info.RowBytes)
			throw new ArgumentException("Pixmap is disposed, empty or unreadable.", name);
	}

	private static void ValidateDimensions(int width, int height, int otherWidth, int otherHeight)
	{
		if (width != otherWidth || height != otherHeight)
			throw new InvalidOperationException($"Unable to compare images of different sizes: {width}x{height} vs {otherWidth}x{otherHeight}.");
		_ = checked(width * height);
		_ = checked((long)width * height * 4);
	}

	private static void ValidateStyle(SKPixelDifferenceStyle style)
	{
		if (style != SKPixelDifferenceStyle.BinaryMask &&
			style != SKPixelDifferenceStyle.ChannelDelta &&
			style != SKPixelDifferenceStyle.ThresholdOverlay)
			throw new ArgumentOutOfRangeException(nameof(style));
	}
}
