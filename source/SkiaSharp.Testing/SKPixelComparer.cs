// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;

namespace SkiaSharp.Testing;

/// <summary>Compares two decoded images, bitmaps, or pixmaps after normalization to tightly packed BGRA8888.</summary>
/// <remarks>Inputs and optional tolerance and ignore masks are borrowed for a synchronous call; do not mutate or dispose them concurrently. Results retain no native inputs. Difference images are independent, caller-owned, and must be disposed.</remarks>
public static class SKPixelComparer
{
	/// <summary>Compares two images with snapshotted per-channel tolerances, optional tolerance and ignore masks, and match gates over nonignored coverage.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, ignoreMask, settings);
	}

	/// <summary>Compares two bitmaps with snapshotted per-channel tolerances, optional tolerance and ignore masks, and match gates over nonignored coverage.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, ignoreMask, settings);
	}

	/// <summary>Compares two pixmaps with snapshotted per-channel tolerances, optional tolerance and ignore masks, and match gates over nonignored coverage.</summary>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A configured alpha type or budget is invalid.</exception>
	public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return ComparePixels(a, b, mask, ignoreMask, settings);
	}

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and optional borrowed tolerance and ignore masks.</summary>
	/// <remarks>The default ThresholdOverlay flattens the second (actual) input onto white, then blends red mismatch, amber (255,200,0) tolerated selected-channel difference, or gray (128,128,128) ignored highlights at 50%, rounding half-channel values upward. Exact selected-channel matches retain the flattened actual RGB, including disabled channels. ThresholdMap instead uses opaque red, amber, dimmed actual RGB, and gray classifications. Ignored pixels are black in BinaryMask and ChannelDifference. All styles output BGRA8888 with alpha 255.</remarks>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, ignoreMask, style, settings);
	}

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and optional borrowed tolerance and ignore masks.</summary>
	/// <remarks>The default ThresholdOverlay flattens the second (actual) input onto white, then blends red mismatch, amber (255,200,0) tolerated selected-channel difference, or gray (128,128,128) ignored highlights at 50%, rounding half-channel values upward. Exact selected-channel matches retain the flattened actual RGB, including disabled channels. ThresholdMap instead uses opaque red, amber, dimmed actual RGB, and gray classifications. Ignored pixels are black in BinaryMask and ChannelDifference. All styles output BGRA8888 with alpha 255.</remarks>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, ignoreMask, style, settings);
	}

	/// <summary>Creates a caller-owned opaque difference image in the selected style, using snapshotted options and optional borrowed tolerance and ignore masks.</summary>
	/// <remarks>The default ThresholdOverlay flattens the second (actual) input onto white, then blends red mismatch, amber (255,200,0) tolerated selected-channel difference, or gray (128,128,128) ignored highlights at 50%, rounding half-channel values upward. Exact selected-channel matches retain the flattened actual RGB, including disabled channels. ThresholdMap instead uses opaque red, amber, dimmed actual RGB, and gray classifications. Ignored pixels are black in BinaryMask and ChannelDifference. All styles output BGRA8888 with alpha 255.</remarks>
	/// <exception cref="ArgumentNullException">An input is null.</exception>
	/// <exception cref="ArgumentException">An input or mask is empty or disposed, all channels are disabled, or IgnoreMask excludes every pixel (parameter IgnoreMask).</exception>
	/// <exception cref="InvalidOperationException">The dimensions differ or pixel readback fails.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The style, alpha type, or budget is invalid.</exception>
	public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null)
	{
		ValidatePair(first, second);
		ValidateStyle(style);
		var settings = new Settings(options);
		using var a = Normalize(first, settings);
		using var b = Normalize(second, settings);
		using var mask = NormalizeMask(first.Width, first.Height, settings);
		using var ignoreMask = ReadIgnoreMask(first.Width, first.Height, settings);
		return DifferenceImage(a, b, mask, ignoreMask, style, settings);
	}

	private static SKBitmap? NormalizeMask(int width, int height, Settings settings)
	{
		if (settings.Mask == null)
			return null;
		ValidateMask(width, height, settings.Mask);
		return Normalize(settings.Mask, settings);
	}

	private static SKBitmap? ReadIgnoreMask(int width, int height, Settings settings)
	{
		if (settings.IgnoreMask == null)
			return null;
		Validate(settings.IgnoreMask, nameof(SKPixelComparerOptions.IgnoreMask));
		ValidateDimensions(width, height, settings.IgnoreMask.Width, settings.IgnoreMask.Height);
		return ReadPixels(settings.IgnoreMask, SKAlphaType.Unpremul);
	}

	private static SKPixelComparisonResult ComparePixels(SKBitmap first, SKBitmap second, SKBitmap? mask, SKBitmap? ignoreMask, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		var comparedPixels = 0;
		var mismatchedPixels = 0;
		var maxChannelDifference = 0;
		var rawSumAbsoluteError = 0L;
		var rawSumSquaredError = 0L;
		var thresholdedSumAbsoluteError = 0L;
		var thresholdedSumSquaredError = 0L;
		var left = first.Width;
		var top = first.Height;
		var right = 0;
		var bottom = 0;

		using var aPixmap = first.PeekPixels();
		using var bPixmap = second.PeekPixels();
		using var mPixmap = mask?.PeekPixels();
		using var iPixmap = ignoreMask?.PeekPixels();
		if (aPixmap == null || bPixmap == null || (mask != null && mPixmap == null) || (ignoreMask != null && iPixmap == null))
			throw new InvalidOperationException("Unable to access normalized pixels.");
		var a = aPixmap.GetPixelSpan<SKColor>();
		var b = bPixmap.GetPixelSpan<SKColor>();
		var m = mPixmap == null ? Span<SKColor>.Empty : mPixmap.GetPixelSpan<SKColor>();
		var ignored = iPixmap == null ? Span<SKColor>.Empty : iPixmap.GetPixelSpan<SKColor>();

		for (var i = 0; i < totalPixels; i++)
		{
			if (ignoreMask != null && IsIgnored(ignored[i]))
				continue;
			comparedPixels++;
			var delta = Evaluate(a[i], b[i], mask == null ? (SKColor?)null : m[i], settings);
			rawSumAbsoluteError = checked(rawSumAbsoluteError + delta.RawSumAbsoluteError);
			rawSumSquaredError = checked(rawSumSquaredError + delta.RawSumSquaredError);
			thresholdedSumAbsoluteError = checked(thresholdedSumAbsoluteError + delta.ThresholdedSumAbsoluteError);
			thresholdedSumSquaredError = checked(thresholdedSumSquaredError + delta.ThresholdedSumSquaredError);
			if (delta.Mismatched)
			{
				mismatchedPixels++;
				var x = i % first.Width;
				var y = i / first.Width;
				left = Math.Min(left, x);
				top = Math.Min(top, y);
				right = Math.Max(right, x + 1);
				bottom = Math.Max(bottom, y + 1);
			}
			maxChannelDifference = Math.Max(maxChannelDifference, delta.MaxChannelDifference);
		}

		ValidateCoverage(comparedPixels);
		var raw = new SKPixelComparisonMetrics(comparedPixels, settings.ChannelCount, rawSumAbsoluteError, rawSumSquaredError);
		var thresholded = new SKPixelComparisonMetrics(comparedPixels, settings.ChannelCount, thresholdedSumAbsoluteError, thresholdedSumSquaredError);
		var isMatch = (!settings.MismatchBudget.HasValue && !settings.MaxNormalizedRootMeanSquaredError.HasValue && !settings.MaxNormalizedMeanAbsoluteError.HasValue
				? mismatchedPixels == 0
				: (!settings.MismatchBudget.HasValue || settings.MismatchBudget.Value.Allows(mismatchedPixels, comparedPixels))) &&
			(!settings.MaxNormalizedRootMeanSquaredError.HasValue ||
				raw.NormalizedRootMeanSquaredError <= settings.MaxNormalizedRootMeanSquaredError.Value) &&
			(!settings.MaxNormalizedMeanAbsoluteError.HasValue ||
				raw.NormalizedMeanAbsoluteError <= settings.MaxNormalizedMeanAbsoluteError.Value);
		return new SKPixelComparisonResult(totalPixels, comparedPixels, settings.ChannelCount, mismatchedPixels, maxChannelDifference,
			mismatchedPixels == 0 ? (SKRectI?)null : new SKRectI(left, top, right, bottom), raw, thresholded, isMatch);
	}

	private static SKImage DifferenceImage(SKBitmap first, SKBitmap second, SKBitmap? mask, SKBitmap? ignoreMask, SKPixelDifferenceStyle style, Settings settings)
	{
		var totalPixels = checked(first.Width * first.Height);
		var comparedPixels = 0;
		using var aPixmap = first.PeekPixels();
		using var bPixmap = second.PeekPixels();
		using var mPixmap = mask?.PeekPixels();
		using var iPixmap = ignoreMask?.PeekPixels();
		if (aPixmap == null || bPixmap == null || (mask != null && mPixmap == null) || (ignoreMask != null && iPixmap == null))
			throw new InvalidOperationException("Unable to access normalized pixels.");
		var a = aPixmap.GetPixelSpan<SKColor>();
		var b = bPixmap.GetPixelSpan<SKColor>();
		var m = mPixmap == null ? Span<SKColor>.Empty : mPixmap.GetPixelSpan<SKColor>();
		var ignored = iPixmap == null ? Span<SKColor>.Empty : iPixmap.GetPixelSpan<SKColor>();

		using var bitmap = Allocate(new SKImageInfo(first.Width, first.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
		using var pixmap = bitmap.PeekPixels();
		if (pixmap == null)
			throw new InvalidOperationException("Unable to access difference pixels.");
		var pixels = pixmap.GetPixelSpan<SKColor>();
		for (var i = 0; i < totalPixels; i++)
		{
			if (ignoreMask != null && IsIgnored(ignored[i]))
			{
				pixels[i] = style switch
				{
					SKPixelDifferenceStyle.ThresholdMap => new SKColor(128, 128, 128),
					SKPixelDifferenceStyle.ThresholdOverlay => BlendOverlay(b[i], new SKColor(128, 128, 128), settings.AlphaType),
					_ => SKColors.Black,
				};
				continue;
			}
			comparedPixels++;
			var delta = Evaluate(a[i], b[i], mask == null ? (SKColor?)null : m[i], settings);
			pixels[i] = style switch
			{
				SKPixelDifferenceStyle.BinaryMask => delta.Mismatched ? SKColors.White : SKColors.Black,
				SKPixelDifferenceStyle.ChannelDifference => new SKColor((byte)delta.Red, (byte)delta.Green, (byte)delta.Blue),
				SKPixelDifferenceStyle.ThresholdMap => delta.Mismatched ? SKColors.Red
					: delta.MaxChannelDifference > 0 ? new SKColor(255, 200, 0)
					: new SKColor((byte)(b[i].Red / 4), (byte)(b[i].Green / 4), (byte)(b[i].Blue / 4)),
				_ => BlendOverlay(b[i], delta.Mismatched ? SKColors.Red
					: delta.MaxChannelDifference > 0 ? new SKColor(255, 200, 0) : (SKColor?)null, settings.AlphaType),
			};
		}

		ValidateCoverage(comparedPixels);
		return SKImage.FromBitmap(bitmap) ?? throw new InvalidOperationException("Unable to create difference image.");
	}

	private static SKColor BlendOverlay(SKColor actual, SKColor? classification, SKAlphaType alphaType)
	{
		var red = Flatten(actual.Red);
		var green = Flatten(actual.Green);
		var blue = Flatten(actual.Blue);
		return classification.HasValue
			? new SKColor((byte)((red + classification.Value.Red + 1) / 2),
				(byte)((green + classification.Value.Green + 1) / 2),
				(byte)((blue + classification.Value.Blue + 1) / 2))
			: new SKColor(red, green, blue);

		byte Flatten(byte channel) => alphaType == SKAlphaType.Premul
			? (byte)Math.Min(255, channel + 255 - actual.Alpha)
			: (byte)((channel * actual.Alpha + 255 * (255 - actual.Alpha) + 127) / 255);
	}

	private static bool IsIgnored(SKColor pixel) => pixel.Red != 0 || pixel.Green != 0 || pixel.Blue != 0;

	private static void ValidateCoverage(int comparedPixels)
	{
		if (comparedPixels == 0)
			throw new ArgumentException("IgnoreMask must leave at least one pixel compared.", nameof(SKPixelComparerOptions.IgnoreMask));
	}

	private static PixelDifference Evaluate(SKColor first, SKColor second, SKColor? mask, Settings settings)
	{
		var r = Math.Abs(second.Red - first.Red);
		var g = Math.Abs(second.Green - first.Green);
		var b = Math.Abs(second.Blue - first.Blue);
		var a = Math.Abs(second.Alpha - first.Alpha);
		var rawSumAbsoluteError = 0;
		var rawSumSquaredError = 0L;
		var thresholdedSumAbsoluteError = 0;
		var thresholdedSumSquaredError = 0L;
		var maxChannelDifference = 0;
		Accumulate(r, settings.RedTolerance, mask?.Red, ref rawSumAbsoluteError, ref rawSumSquaredError,
			ref thresholdedSumAbsoluteError, ref thresholdedSumSquaredError, ref maxChannelDifference);
		Accumulate(g, settings.GreenTolerance, mask?.Green, ref rawSumAbsoluteError, ref rawSumSquaredError,
			ref thresholdedSumAbsoluteError, ref thresholdedSumSquaredError, ref maxChannelDifference);
		Accumulate(b, settings.BlueTolerance, mask?.Blue, ref rawSumAbsoluteError, ref rawSumSquaredError,
			ref thresholdedSumAbsoluteError, ref thresholdedSumSquaredError, ref maxChannelDifference);
		Accumulate(a, settings.AlphaTolerance, mask?.Alpha, ref rawSumAbsoluteError, ref rawSumSquaredError,
			ref thresholdedSumAbsoluteError, ref thresholdedSumSquaredError, ref maxChannelDifference);
		return new PixelDifference(r, g, b, maxChannelDifference, rawSumAbsoluteError, rawSumSquaredError,
			thresholdedSumAbsoluteError, thresholdedSumSquaredError);
	}

	private static void Accumulate(int difference, SKPixelTolerance? allowance, byte? maskAllowance, ref int rawSumAbsoluteError, ref long rawSumSquaredError,
		ref int thresholdedSumAbsoluteError, ref long thresholdedSumSquaredError, ref int maxChannelDifference)
	{
		if (!allowance.HasValue)
			return;
		rawSumAbsoluteError += difference;
		rawSumSquaredError += (long)difference * difference;
		maxChannelDifference = Math.Max(maxChannelDifference, difference);
		if (!allowance.Value.Allows(difference) && (!maskAllowance.HasValue || difference > maskAllowance.Value))
		{
			thresholdedSumAbsoluteError += difference;
			thresholdedSumSquaredError += (long)difference * difference;
		}
	}

	private readonly struct PixelDifference
	{
		public PixelDifference(int red, int green, int blue, int maxChannelDifference, int rawSumAbsoluteError, long rawSumSquaredError,
			int thresholdedSumAbsoluteError, long thresholdedSumSquaredError)
		{
			Red = red;
			Green = green;
			Blue = blue;
			MaxChannelDifference = maxChannelDifference;
			RawSumAbsoluteError = rawSumAbsoluteError;
			RawSumSquaredError = rawSumSquaredError;
			ThresholdedSumAbsoluteError = thresholdedSumAbsoluteError;
			ThresholdedSumSquaredError = thresholdedSumSquaredError;
		}

		public int Red { get; }
		public int Green { get; }
		public int Blue { get; }
		public int MaxChannelDifference { get; }
		public int RawSumAbsoluteError { get; }
		public long RawSumSquaredError { get; }
		public int ThresholdedSumAbsoluteError { get; }
		public long ThresholdedSumSquaredError { get; }
		public bool Mismatched => ThresholdedSumAbsoluteError > 0;
	}

	private readonly struct Settings
	{
		public Settings(SKPixelComparerOptions? options)
		{
			RedTolerance = options == null ? SKPixelTolerance.Exact : options.RedTolerance;
			GreenTolerance = options == null ? SKPixelTolerance.Exact : options.GreenTolerance;
			BlueTolerance = options == null ? SKPixelTolerance.Exact : options.BlueTolerance;
			AlphaTolerance = options == null ? SKPixelTolerance.Exact : options.AlphaTolerance;
			ChannelCount = (RedTolerance.HasValue ? 1 : 0) + (GreenTolerance.HasValue ? 1 : 0) +
				(BlueTolerance.HasValue ? 1 : 0) + (AlphaTolerance.HasValue ? 1 : 0);
			AlphaType = options?.AlphaType ?? SKAlphaType.Unpremul;
			Mask = options?.ToleranceMask;
			IgnoreMask = options?.IgnoreMask;
			MismatchBudget = options?.MismatchBudget;
			MaxNormalizedRootMeanSquaredError = options?.MaxNormalizedRootMeanSquaredError;
			MaxNormalizedMeanAbsoluteError = options?.MaxNormalizedMeanAbsoluteError;
			if (ChannelCount == 0)
				throw new ArgumentException("At least one channel must be enabled.", nameof(options));
			if (AlphaType != SKAlphaType.Unpremul && AlphaType != SKAlphaType.Premul)
				throw new ArgumentOutOfRangeException(nameof(options), "AlphaType must be Unpremul or Premul.");
			if (InvalidFraction(MaxNormalizedRootMeanSquaredError))
				throw new ArgumentOutOfRangeException(nameof(options), "MaxNormalizedRootMeanSquaredError must be finite and between 0 and 1.");
			if (InvalidFraction(MaxNormalizedMeanAbsoluteError))
				throw new ArgumentOutOfRangeException(nameof(options), "MaxNormalizedMeanAbsoluteError must be finite and between 0 and 1.");
		}

		public SKPixelTolerance? RedTolerance { get; }
		public SKPixelTolerance? GreenTolerance { get; }
		public SKPixelTolerance? BlueTolerance { get; }
		public SKPixelTolerance? AlphaTolerance { get; }
		public int ChannelCount { get; }
		public SKAlphaType AlphaType { get; }
		public SKImage? Mask { get; }
		public SKImage? IgnoreMask { get; }
		public SKPixelMismatchBudget? MismatchBudget { get; }
		public double? MaxNormalizedRootMeanSquaredError { get; }
		public double? MaxNormalizedMeanAbsoluteError { get; }

		private static bool InvalidFraction(double? value) =>
			value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value < 0 || value > 1);
	}

	private static SKBitmap Normalize(SKImage source, Settings settings)
	{
		var bitmap = ReadPixels(source, settings.AlphaType);
		try
		{
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

	private static SKBitmap ReadPixels(SKImage source, SKAlphaType alphaType)
	{
		var bitmap = Allocate(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, alphaType));
		try
		{
			using var destination = bitmap.PeekPixels();
			if (destination == null || !source.ReadPixels(destination))
				throw new InvalidOperationException("Unable to read image pixels.");
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
			style != SKPixelDifferenceStyle.ChannelDifference &&
			style != SKPixelDifferenceStyle.ThresholdMap &&
			style != SKPixelDifferenceStyle.ThresholdOverlay)
			throw new ArgumentOutOfRangeException(nameof(style));
	}
}
